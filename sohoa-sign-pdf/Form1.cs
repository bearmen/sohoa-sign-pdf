using System.Diagnostics;
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
        private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 5000 };

        public Form1(
            ConfigurationService configurationService,
            AppLogger logger,
            TokenService tokenService,
            SigningQueueService signingQueueService,
            LocalApiServer localApiServer,
            AutoStartService autoStartService)
        {
            InitializeComponent();
            _configurationService = configurationService;
            _logger = logger;
            _tokenService = tokenService;
            _signingQueueService = signingQueueService;
            _localApiServer = localApiServer;
            _autoStartService = autoStartService;

            notifyIcon1.Icon = SystemIcons.Application;
            notifyIcon1.Visible = true;
            chkAutoStart.Checked = _autoStartService.IsEnabled();
            txtApiPort.Text = _configurationService.Current.ApiPort.ToString();
            txtAllowedOrigins.Text = string.Join(Environment.NewLine, _configurationService.Current.AllowedOrigins);
            txtTokenDllPath.Text = _configurationService.Current.TokenLibraryPath;
            txtApiKey.Text = _configurationService.Current.ApiKey;

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
            AppendLog($"Config file: {_configurationService.ConfigPath}");
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            _statusTimer.Stop();
            _logger.LogReceived -= OnLogReceived;
            _tokenService.StatusChanged -= OnTokenStatusChanged;
            notifyIcon1.Visible = false;
            await _localApiServer.StopAsync();
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState != FormWindowState.Minimized)
            {
                return;
            }

            Hide();
            notifyIcon1.ShowBalloonTip(2000, "sohoa-sign-pdf", "?ng d?ng v?n ch?y ? system tray.", ToolTipIcon.Info);
        }

        private async Task StartApiIfNeededAsync()
        {
            try
            {
                await _localApiServer.StartAsync();
                lblApiStatus.Text = $"API: Running on 127.0.0.1:{_configurationService.Current.ApiPort}";
            }
            catch (Exception ex)
            {
                lblApiStatus.Text = "API: Error";
                _logger.Error("Không th? kh?i ??ng local API.", ex);
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
                _logger.Error("Refresh token status th?t b?i.", ex);
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
                _logger.Warning($"Không th? t?i certificates: {ex.Message}");
            }
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
            lblTokenStatus.Text = $"Token: {status.Message}";
            lblLoginStatus.Text = $"Login: {(status.IsLoggedIn ? "?ã login" : "Ch?a login")}";
            lblTokenInfo.Text = $"Label: {status.TokenLabel ?? "N/A"} | Serial: {status.SerialNumber ?? "N/A"}";

            notifyIcon1.Text = $"sohoa-sign-pdf - {status.Message}";
            notifyIcon1.Icon = status.State switch
            {
                TokenHealthState.Ready => SystemIcons.Shield,
                TokenHealthState.TokenMissing => SystemIcons.Warning,
                TokenHealthState.Error => SystemIcons.Error,
                _ => SystemIcons.Information
            };
        }

        private void AppendLog(string message)
        {
            txtLogs.AppendText(message + Environment.NewLine);
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
                MessageBox.Show(ex.Message, "Login th?t b?i", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(result.Error, "Ký th?t b?i", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtSignatureResult.Text = result.SignatureBase64;
            txtPin.Clear();
        }

        private async void btnRestartApi_Click(object sender, EventArgs e)
        {
            await _localApiServer.StopAsync();
            await StartApiIfNeededAsync();
        }

        private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _autoStartService.SetEnabled(chkAutoStart.Checked);
                var config = _configurationService.Current;
                config.AutoStart = chkAutoStart.Checked;
                _configurationService.Save(config);
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
            await _localApiServer.StopAsync();
            await StartApiIfNeededAsync();
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
                Title = "Ch?n PKCS#11 DLL"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            txtTokenDllPath.Text = dialog.FileName;
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtApiPort.Text.Trim(), out var port) || port is < 1024 or > 65535)
            {
                MessageBox.Show("Port không h?p l?.", "Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var config = _configurationService.Current;
            config.ApiPort = port;
            config.ApiKey = txtApiKey.Text.Trim();
            config.TokenLibraryPath = txtTokenDllPath.Text.Trim();
            config.AutoStart = chkAutoStart.Checked;
            config.AllowedOrigins = txtAllowedOrigins.Text
                .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            _configurationService.Save(config);
            _logger.Info("?ã l?u c?u hình. Hãy restart API ?? áp d?ng port m?i.");
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
    }
}
