using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using sohoa_sign_pdf.Configuration;
using sohoa_sign_pdf.Models;
using sohoa_sign_pdf.Services;

namespace sohoa_sign_pdf
{
    public partial class Form1 : Form
    {
        private readonly ConfigurationService _configurationService;
        private readonly AppLogger _logger;
        private readonly TokenService _tokenService;
        private readonly SigningQueueService _signingQueueService;
        private readonly LocalApiServer _localApiServer;
        private readonly AutoStartService _autoStartService;
        private readonly Pkcs11DllDiscoveryService _pkcs11DllDiscoveryService;
        private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 5000 };
        private readonly List<Button> _guidedButtons = [];
        private List<Pkcs11DllCandidate> _lastDetectedDllCandidates = [];
        private TokenStatusInfo _lastTokenStatus = new() { State = TokenHealthState.Unknown, Message = "Chưa có trạng thái token." };
        private Icon? _trayCustomIcon;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public Form1(
            ConfigurationService configurationService,
            AppLogger logger,
            TokenService tokenService,
            SigningQueueService signingQueueService,
            LocalApiServer localApiServer,
            AutoStartService autoStartService,
            Pkcs11DllDiscoveryService pkcs11DllDiscoveryService)
        {
            InitializeComponent();
            _configurationService = configurationService;
            _logger = logger;
            _tokenService = tokenService;
            _signingQueueService = signingQueueService;
            _localApiServer = localApiServer;
            _autoStartService = autoStartService;
            _pkcs11DllDiscoveryService = pkcs11DllDiscoveryService;

            _trayCustomIcon = LoadTrayIconFromImages();
            notifyIcon1.Icon = _trayCustomIcon ?? SystemIcons.Application;
            notifyIcon1.Visible = true;
            chkAutoStart.Checked = _autoStartService.IsEnabled();
            chkAllowLanClients.Checked = _configurationService.Current.AllowLanClients;
            txtApiPort.Text = _configurationService.Current.ApiPort.ToString();
            txtAllowedOrigins.Text = string.Join(Environment.NewLine, _configurationService.Current.AllowedOrigins);
            txtTokenDllPath.Text = _configurationService.Current.TokenLibraryPath;
            txtApiKey.Text = _configurationService.Current.ApiKey;

            _guidedButtons.AddRange([btnAutoDetectDll, btnSaveConfig, btnLogin, btnRefresh, btnSignTest]);
            txtTokenDllPath.TextChanged += (_, _) => UpdateNextActionHighlight();

            _logger.LogReceived += OnLogReceived;
            _tokenService.StatusChanged += OnTokenStatusChanged;
            _statusTimer.Tick += async (_, _) => await RefreshStatusAsync();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await StartApiIfNeededAsync();
            await RefreshCertificatesAsync();
            await RefreshStatusAsync();
            _statusTimer.Start();
            AppendLog($"Tệp cấu hình: {_configurationService.ConfigPath}");
            AppendQuickStartGuideToLog();

            if (!string.IsNullOrWhiteSpace(txtTokenDllPath.Text))
            {
                txtTokenDllPath.Items.Clear();
                txtTokenDllPath.Items.Add(txtTokenDllPath.Text);
            }

            ApplyDllFilter();
            UpdateNextActionHighlight();
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            _statusTimer.Stop();
            _logger.LogReceived -= OnLogReceived;
            _tokenService.StatusChanged -= OnTokenStatusChanged;
            notifyIcon1.Visible = false;
            await _localApiServer.StopAsync();
            _trayCustomIcon?.Dispose();
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState != FormWindowState.Minimized)
            {
                return;
            }

            notifyIcon1.Icon = _trayCustomIcon ?? SystemIcons.Application;
            Hide();
            notifyIcon1.ShowBalloonTip(2000, "sohoa-sign-pdf", "Ứng dụng vẫn chạy ở khay hệ thống.", ToolTipIcon.Info);
        }

        private async Task StartApiIfNeededAsync()
        {
            try
            {
                await _localApiServer.StartAsync();
                var host = _configurationService.Current.AllowLanClients ? "0.0.0.0" : "127.0.0.1";
                lblApiStatus.Text = $"API: Đang chạy tại {host}:{_configurationService.Current.ApiPort}";
            }
            catch (Exception ex)
            {
                lblApiStatus.Text = "API: Lỗi";
                _logger.Error("Không thể khởi động local API.", ex);
            }
        }

        private async Task RefreshStatusAsync()
        {
            try
            {
                var status = await _tokenService.GetStatusAsync();
                ApplyTokenStatus(status);
                if (!_localApiServer.IsRunning)
                {
                    await StartApiIfNeededAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Làm mới trạng thái token thất bại.", ex);
            }
        }

        private async Task RefreshCertificatesAsync()
        {
            lvCertificates.Items.Clear();
            try
            {
                var certificates = await _tokenService.GetCertificatesAsync();
                foreach (var cert in certificates)
                {
                    var item = new ListViewItem(cert.Subject);
                    item.SubItems.Add(cert.SerialNumber);
                    item.SubItems.Add(cert.NotAfter.ToString("yyyy-MM-dd HH:mm"));
                    item.SubItems.Add(cert.Id);
                    item.Tag = cert;
                    lvCertificates.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Không thể tải certificates: {ex.Message}");
            }

            UpdateNextActionHighlight();
        }

        private void OnLogReceived(string line)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AppendLog(line));
                return;
            }

            AppendLog(line);
        }

        private void OnTokenStatusChanged(TokenStatusInfo status)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => ApplyTokenStatus(status));
                return;
            }

            ApplyTokenStatus(status);
        }

        private void ApplyTokenStatus(TokenStatusInfo status)
        {
            _lastTokenStatus = status;
            lblTokenStatus.Text = $"Token: {status.Message}";
            lblLoginStatus.Text = $"Đăng nhập: {(status.IsLoggedIn ? "Đã đăng nhập" : "Chưa đăng nhập")}";
            lblTokenInfo.Text = $"Nhãn: {status.TokenLabel ?? "N/A"} | Serial: {status.SerialNumber ?? "N/A"}";

            notifyIcon1.Text = $"sohoa-sign-pdf - {status.Message}";
            notifyIcon1.Icon = _trayCustomIcon ?? status.State switch
            {
                TokenHealthState.TokenMissing => SystemIcons.Warning,
                TokenHealthState.Error => SystemIcons.Error,
                _ => SystemIcons.Application
            };

            UpdateNextActionHighlight();
        }

        private void AppendQuickStartGuideToLog()
        {
            AppendLog("=== HƯỚNG DẪN BẮT ĐẦU NHANH ===");
            AppendLog("Bước 1: Chọn PKCS#11 DLL (bấm 'Tự động dò DLL' hoặc '...').");
            AppendLog("Bước 2: Bấm 'Lưu cấu hình'.");
            AppendLog("Bước 3: Nhập PIN và bấm 'Đăng nhập token'.");
            AppendLog("Bước 4: Bấm 'Làm mới' để nạp chứng thư.");
            AppendLog("Bước 5: Ký thử ở khu vực 'Ký nhanh (test)' hoặc gọi API từ FE/BE.");
        }

        private void AppendLog(string message)
        {
            txtLogs.AppendText(message + Environment.NewLine);
        }

        private void UpdateNextActionHighlight()
        {
            var (nextButton, nextStep) = DetermineNextStep();
            ApplyButtonHighlight(nextButton);
            lblQuickGuide.Text =
                "1) Tự động dò/chọn DLL\r\n" +
                "2) Lưu cấu hình\r\n" +
                "3) Nhập PIN và đăng nhập\r\n" +
                "4) Làm mới + ký thử\r\n" +
                $"➡ Gợi ý tiếp theo: {nextStep}";
        }

        private (Button? Button, string Step) DetermineNextStep()
        {
            var dllPath = txtTokenDllPath.Text.Trim();
            if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
            {
                return (btnAutoDetectDll, "Chọn PKCS#11 DLL trước (ưu tiên bấm 'Tự động dò DLL').");
            }

            if (HasPendingConfigChange())
            {
                return (btnSaveConfig, "Bấm 'Lưu cấu hình' để áp dụng thông tin hiện tại.");
            }

            if (!_lastTokenStatus.IsTokenPresent)
            {
                return (btnRefresh, "Cắm USB Token rồi bấm 'Làm mới'.");
            }

            if (!_lastTokenStatus.IsLoggedIn)
            {
                return (btnLogin, "Nhập PIN và bấm 'Đăng nhập token'.");
            }

            if (lvCertificates.Items.Count == 0)
            {
                return (btnRefresh, "Bấm 'Làm mới' để nạp danh sách chứng thư.");
            }

            return (btnSignTest, "Có thể ký thử dữ liệu để kiểm tra luồng ký.");
        }

        private bool HasPendingConfigChange()
        {
            var config = _configurationService.Current;
            if (!int.TryParse(txtApiPort.Text.Trim(), out var typedPort))
            {
                typedPort = config.ApiPort;
            }

            var typedOrigins = txtAllowedOrigins.Text
                .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return !string.Equals(config.TokenLibraryPath, txtTokenDllPath.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                || config.ApiPort != typedPort
                || !string.Equals(config.ApiKey, txtApiKey.Text.Trim(), StringComparison.Ordinal)
                || !config.AllowedOrigins.SequenceEqual(typedOrigins, StringComparer.OrdinalIgnoreCase)
                || config.AutoStart != chkAutoStart.Checked
                || config.AllowLanClients != chkAllowLanClients.Checked;
        }

        private void ApplyButtonHighlight(Button? target)
        {
            foreach (var button in _guidedButtons)
            {
                button.UseVisualStyleBackColor = true;
                button.FlatStyle = FlatStyle.Standard;
            }

            if (target is null)
            {
                return;
            }

            target.FlatStyle = FlatStyle.Flat;
            target.FlatAppearance.BorderColor = Color.DarkOrange;
            target.FlatAppearance.BorderSize = 2;
            target.BackColor = Color.FromArgb(255, 245, 204);
        }

        private Icon? LoadTrayIconFromImages()
        {
            var rootCandidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Images"),
                Path.Combine(AppContext.BaseDirectory, "images"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Images")),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "images"))
            };

            var iconNames = new[]
            {
                "tray.ico",
                "app.ico",
                "logo.ico",
                "sohoa-sign-pdf.ico",
                "tray.png",
                "app.png",
                "logo.png"
            };

            foreach (var root in rootCandidates.Where(Directory.Exists))
            {
                foreach (var iconName in iconNames)
                {
                    var file = Path.Combine(root, iconName);
                    if (!File.Exists(file))
                    {
                        continue;
                    }

                    try
                    {
                        if (Path.GetExtension(file).Equals(".ico", StringComparison.OrdinalIgnoreCase))
                        {
                            return new Icon(file);
                        }

                        using var bitmap = new Bitmap(file);
                        var hIcon = bitmap.GetHicon();
                        try
                        {
                            using var temp = Icon.FromHandle(hIcon);
                            return (Icon)temp.Clone();
                        }
                        finally
                        {
                            DestroyIcon(hIcon);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        private CertificateInfo? GetSelectedCertificate()
        {
            return lvCertificates.SelectedItems.Count > 0 ? lvCertificates.SelectedItems[0].Tag as CertificateInfo : null;
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await RefreshCertificatesAsync();
            await RefreshStatusAsync();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                await _tokenService.LoginAsync(txtPin.Text.Trim());
                txtPin.Clear();
                await RefreshCertificatesAsync();
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnSignTest_Click(object sender, EventArgs e)
        {
            var cert = GetSelectedCertificate();
            var request = new SignRequest
            {
                Data = txtTestPayload.Text,
                Type = "raw",
                CertId = cert?.Id,
                Pin = txtPin.Text.Trim(),
                HashAlgorithm = "SHA256"
            };

            var result = await _signingQueueService.SignDataAsync(request);
            if (!result.Success)
            {
                MessageBox.Show(result.Error, "Ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtSignatureResult.Text = result.SignatureBase64;
            txtPin.Clear();
        }

        private async void btnRestartApi_Click(object sender, EventArgs e)
        {
            ApplyApiSettingsFromUi();
            await _localApiServer.StopAsync();
            await StartApiIfNeededAsync();
            UpdateNextActionHighlight();
        }

        private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _autoStartService.SetEnabled(chkAutoStart.Checked);
                var config = _configurationService.Current;
                config.AutoStart = chkAutoStart.Checked;
                _configurationService.Save(config);
                UpdateNextActionHighlight();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Auto start", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void menuOpenDashboard_Click(object sender, EventArgs e)
        {
            notifyIcon1_DoubleClick(sender, e);
        }

        private async void menuRestartService_Click(object sender, EventArgs e)
        {
            ApplyApiSettingsFromUi();
            await _localApiServer.StopAsync();
            await StartApiIfNeededAsync();
            UpdateNextActionHighlight();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnBrowseDll_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = "Chọn PKCS#11 DLL"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var selectedPath = dialog.FileName;
            if (!_lastDetectedDllCandidates.Any(x => x.FilePath.Equals(selectedPath, StringComparison.OrdinalIgnoreCase)))
            {
                _lastDetectedDllCandidates.Insert(0, new Pkcs11DllCandidate
                {
                    FilePath = selectedPath,
                    FileName = Path.GetFileName(selectedPath),
                    CanLoad = true,
                    Details = "Người dùng chọn thủ công.",
                    Score = 1000
                });
            }

            ApplyDllFilter(selectedPath);
            UpdateNextActionHighlight();
        }

        private async void btnAutoDetectDll_Click(object sender, EventArgs e)
        {
            btnAutoDetectDll.Enabled = false;
            btnAutoDetectDll.Text = "Đang quét...";
            AppendLog("Bắt đầu quét tự động PKCS#11 DLL ở các thư mục phổ biến...");

            var progress = new Progress<string>(AppendLog);
            try
            {
                var candidates = await _pkcs11DllDiscoveryService.DiscoverAsync(progress);
                _lastDetectedDllCandidates = candidates.ToList();

                if (candidates.Count == 0)
                {
                    AppendLog("Không tìm thấy ứng viên PKCS#11 DLL.");
                    MessageBox.Show("Không tìm thấy candidate PKCS#11 DLL nào.", "Tự động dò DLL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var best = SelectBestCandidate(candidates);
                ApplyDllFilter(best?.FilePath);

                AppendLog($"Đã quét xong. Tìm thấy {candidates.Count} ứng viên.");
                if (best is not null)
                {
                    AppendLog($"Đề xuất tốt nhất: {best.FilePath} | score={best.Score} | {best.Details}");
                }

                if (candidates.Count > 1)
                {
                    MessageBox.Show($"Đã tìm thấy {candidates.Count} DLL khả dụng.\nĐã chọn mặc định DLL có điểm cao nhất.\nBạn có thể dùng combobox để chọn DLL khác.",
                        "Tự động dò DLL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Tự động dò PKCS#11 DLL thất bại.", ex);
                MessageBox.Show(ex.Message, "Tự động dò DLL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnAutoDetectDll.Text = "Tự động dò DLL";
                btnAutoDetectDll.Enabled = true;
                UpdateNextActionHighlight();
            }
        }

        private void chkOnlyLoadableDll_CheckedChanged(object sender, EventArgs e)
        {
            ApplyDllFilter(txtTokenDllPath.Text.Trim());
            AppendLog(chkOnlyLoadableDll.Checked
                ? "Đang bật lọc: chỉ hiện DLL load được."
                : "Đã tắt lọc: hiển thị tất cả DLL ứng viên.");
            UpdateNextActionHighlight();
        }

        private void btnGenerateApiKey_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Tạo API key mới sẽ làm các client cũ không gọi được API cho đến khi cập nhật key mới.\n\nBạn có chắc chắn muốn tạo API key mới không?",
                "Xác nhận tạo API key",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var apiKey = $"sk_{Convert.ToHexString(RandomNumberGenerator.GetBytes(24))}";
            txtApiKey.Text = apiKey;
            AppendLog("Đã tạo API key mới. Nhớ bấm 'Lưu cấu hình' để lưu.");
            UpdateNextActionHighlight();
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtApiPort.Text.Trim(), out var port) || port is < 1024 or > 65535)
            {
                MessageBox.Show("Cổng không hợp lệ.", "Cấu hình", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ApplyApiSettingsFromUi();
            _logger.Info("Đã lưu cấu hình. Hãy khởi động lại API nếu bạn đổi cổng hoặc đổi chế độ LAN.");
            UpdateNextActionHighlight();
        }

        private void ApplyApiSettingsFromUi()
        {
            var config = _configurationService.Current;
            config.ApiPort = int.TryParse(txtApiPort.Text.Trim(), out var port) ? port : config.ApiPort;
            config.ApiKey = txtApiKey.Text.Trim();
            config.TokenLibraryPath = txtTokenDllPath.Text.Trim();
            config.AutoStart = chkAutoStart.Checked;
            config.AllowLanClients = chkAllowLanClients.Checked;
            config.AllowedOrigins = txtAllowedOrigins.Text
                .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            _configurationService.Save(config);
        }

        private void btnOpenLogs_Click(object sender, EventArgs e)
        {
            var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logsPath);
            Process.Start(new ProcessStartInfo
            {
                FileName = logsPath,
                UseShellExecute = true
            });
        }

        private void ApplyDllFilter(string? preferredPath = null)
        {
            var candidates = GetFilteredCandidates();
            if (candidates.Count == 0)
            {
                txtTokenDllPath.Items.Clear();
                if (!string.IsNullOrWhiteSpace(preferredPath))
                {
                    txtTokenDllPath.Items.Add(preferredPath);
                    txtTokenDllPath.Text = preferredPath;
                }

                return;
            }

            txtTokenDllPath.Items.Clear();
            foreach (var candidate in candidates)
            {
                txtTokenDllPath.Items.Add(candidate.FilePath);
            }

            var selected = candidates.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(preferredPath)
                && x.FilePath.Equals(preferredPath, StringComparison.OrdinalIgnoreCase));

            selected ??= SelectBestCandidate(candidates);
            if (selected is not null)
            {
                txtTokenDllPath.Text = selected.FilePath;
            }
        }

        private List<Pkcs11DllCandidate> GetFilteredCandidates()
        {
            if (_lastDetectedDllCandidates.Count == 0)
            {
                return [];
            }

            if (!chkOnlyLoadableDll.Checked)
            {
                return _lastDetectedDllCandidates
                    .OrderByDescending(x => x.Score)
                    .ToList();
            }

            var loadable = _lastDetectedDllCandidates
                .Where(x => x.CanLoad)
                .OrderByDescending(x => x.HasTokenPresent)
                .ThenByDescending(x => x.Score)
                .ToList();

            if (loadable.Count == 0)
            {
                AppendLog("Không có DLL nào load được, tạm hiển thị lại toàn bộ ứng viên.");
                return _lastDetectedDllCandidates
                    .OrderByDescending(x => x.Score)
                    .ToList();
            }

            return loadable;
        }

        private static Pkcs11DllCandidate? SelectBestCandidate(IEnumerable<Pkcs11DllCandidate> candidates)
        {
            return candidates
                .OrderByDescending(x => x.HasTokenPresent)
                .ThenByDescending(x => x.CanLoad)
                .ThenByDescending(x => x.Score)
                .FirstOrDefault();
        }
    }
}
