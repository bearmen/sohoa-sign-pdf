namespace sohoa_sign_pdf
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Label lblApiStatus;
        private Label lblTokenStatus;
        private Label lblLoginStatus;
        private Label lblTokenInfo;
        private ListView lvCertificates;
        private ColumnHeader colSubject;
        private ColumnHeader colSerial;
        private ColumnHeader colExpiry;
        private ColumnHeader colCertId;
        private TextBox txtPin;
        private Button btnLogin;
        private Button btnRefresh;
        private Button btnRestartApi;
        private CheckBox chkAutoStart;
        private TextBox txtLogs;
        private TextBox txtApiPort;
        private TextBox txtAllowedOrigins;
        private ComboBox txtTokenDllPath;
        private TextBox txtApiKey;
        private Button btnSaveConfig;
        private Button btnBrowseDll;
        private Button btnAutoDetectDll;
        private Button btnGenerateApiKey;
        private Button btnOpenLogs;
        private CheckBox chkOnlyLoadableDll;
        private CheckBox chkAllowLanClients;
        private TextBox txtTestPayload;
        private Button btnSignTest;
        private TextBox txtSignatureResult;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip trayMenuStrip;
        private ToolStripMenuItem menuOpenDashboard;
        private ToolStripMenuItem menuRestartService;
        private ToolStripMenuItem menuExit;
        private Label lblConfigTitle;
        private Label lblCertTitle;
        private Label lblPin;
        private Label lblApiPortTitle;
        private Label lblOriginsTitle;
        private Label lblTokenDllTitle;
        private Label lblApiKeyTitle;
        private Label lblPayloadTitle;
        private Label lblSignatureTitle;
        private GroupBox grpQuickGuide;
        private Label lblQuickGuide;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            lblApiStatus = new Label();
            lblTokenStatus = new Label();
            lblLoginStatus = new Label();
            lblTokenInfo = new Label();
            lvCertificates = new ListView();
            colSubject = new ColumnHeader();
            colSerial = new ColumnHeader();
            colExpiry = new ColumnHeader();
            colCertId = new ColumnHeader();
            txtPin = new TextBox();
            btnLogin = new Button();
            btnRefresh = new Button();
            btnRestartApi = new Button();
            chkAutoStart = new CheckBox();
            txtLogs = new TextBox();
            txtApiPort = new TextBox();
            txtAllowedOrigins = new TextBox();
            txtTokenDllPath = new ComboBox();
            txtApiKey = new TextBox();
            btnSaveConfig = new Button();
            btnBrowseDll = new Button();
            btnAutoDetectDll = new Button();
            btnGenerateApiKey = new Button();
            btnOpenLogs = new Button();
            chkOnlyLoadableDll = new CheckBox();
            chkAllowLanClients = new CheckBox();
            txtTestPayload = new TextBox();
            btnSignTest = new Button();
            txtSignatureResult = new TextBox();
            notifyIcon1 = new NotifyIcon(components);
            trayMenuStrip = new ContextMenuStrip(components);
            menuOpenDashboard = new ToolStripMenuItem();
            menuRestartService = new ToolStripMenuItem();
            menuExit = new ToolStripMenuItem();
            lblConfigTitle = new Label();
            lblCertTitle = new Label();
            lblPin = new Label();
            lblApiPortTitle = new Label();
            lblOriginsTitle = new Label();
            lblTokenDllTitle = new Label();
            lblApiKeyTitle = new Label();
            lblPayloadTitle = new Label();
            lblSignatureTitle = new Label();
            grpQuickGuide = new GroupBox();
            lblQuickGuide = new Label();
            trayMenuStrip.SuspendLayout();
            grpQuickGuide.SuspendLayout();
            SuspendLayout();
            // 
            // lblApiStatus
            // 
            lblApiStatus.AutoSize = true;
            lblApiStatus.Location = new Point(20, 20);
            lblApiStatus.Name = "lblApiStatus";
            lblApiStatus.Size = new Size(105, 15);
            lblApiStatus.TabIndex = 0;
            lblApiStatus.Text = "API: Đang khởi tạo";
            // 
            // lblTokenStatus
            // 
            lblTokenStatus.AutoSize = true;
            lblTokenStatus.Location = new Point(20, 45);
            lblTokenStatus.Name = "lblTokenStatus";
            lblTokenStatus.Size = new Size(118, 15);
            lblTokenStatus.TabIndex = 1;
            lblTokenStatus.Text = "Token: Đang kiểm tra";
            // 
            // lblLoginStatus
            // 
            lblLoginStatus.AutoSize = true;
            lblLoginStatus.Location = new Point(20, 70);
            lblLoginStatus.Name = "lblLoginStatus";
            lblLoginStatus.Size = new Size(159, 15);
            lblLoginStatus.TabIndex = 2;
            lblLoginStatus.Text = "Đăng nhập: Chưa đăng nhập";
            // 
            // lblTokenInfo
            // 
            lblTokenInfo.AutoSize = true;
            lblTokenInfo.Location = new Point(20, 95);
            lblTokenInfo.Name = "lblTokenInfo";
            lblTokenInfo.Size = new Size(129, 15);
            lblTokenInfo.TabIndex = 3;
            lblTokenInfo.Text = "Nhãn: N/A | Serial: N/A";
            // 
            // lvCertificates
            // 
            lvCertificates.Columns.AddRange(new ColumnHeader[] { colSubject, colSerial, colExpiry, colCertId });
            lvCertificates.FullRowSelect = true;
            lvCertificates.GridLines = true;
            lvCertificates.Location = new Point(20, 153);
            lvCertificates.MultiSelect = false;
            lvCertificates.Name = "lvCertificates";
            lvCertificates.Size = new Size(718, 161);
            lvCertificates.TabIndex = 4;
            lvCertificates.UseCompatibleStateImageBehavior = false;
            lvCertificates.View = View.Details;
            // 
            // colSubject
            // 
            colSubject.Text = "Chủ thể";
            colSubject.Width = 240;
            // 
            // colSerial
            // 
            colSerial.Text = "Số serial";
            colSerial.Width = 150;
            // 
            // colExpiry
            // 
            colExpiry.Text = "Ngày hết hạn";
            colExpiry.Width = 120;
            // 
            // colCertId
            // 
            colCertId.Text = "Mã chứng thư";
            colCertId.Width = 180;
            // 
            // txtPin
            // 
            txtPin.Location = new Point(762, 174);
            txtPin.Name = "txtPin";
            txtPin.PasswordChar = '*';
            txtPin.Size = new Size(204, 23);
            txtPin.TabIndex = 5;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(762, 203);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(98, 46);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "Đăng nhập token";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(866, 203);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 46);
            btnRefresh.TabIndex = 7;
            btnRefresh.Text = "Làm mới";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnRestartApi
            // 
            btnRestartApi.Location = new Point(762, 255);
            btnRestartApi.Name = "btnRestartApi";
            btnRestartApi.Size = new Size(204, 30);
            btnRestartApi.TabIndex = 8;
            btnRestartApi.Text = "Khởi động lại API";
            btnRestartApi.UseVisualStyleBackColor = true;
            btnRestartApi.Click += btnRestartApi_Click;
            // 
            // chkAutoStart
            // 
            chkAutoStart.AutoSize = true;
            chkAutoStart.Location = new Point(762, 292);
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Size = new Size(178, 19);
            chkAutoStart.TabIndex = 9;
            chkAutoStart.Text = "Tự khởi động cùng Windows";
            chkAutoStart.UseVisualStyleBackColor = true;
            chkAutoStart.CheckedChanged += chkAutoStart_CheckedChanged;
            // 
            // txtLogs
            // 
            txtLogs.Location = new Point(20, 557);
            txtLogs.Multiline = true;
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.ScrollBars = ScrollBars.Vertical;
            txtLogs.Size = new Size(946, 155);
            txtLogs.TabIndex = 10;
            // 
            // txtApiPort
            // 
            txtApiPort.Location = new Point(148, 365);
            txtApiPort.Name = "txtApiPort";
            txtApiPort.Size = new Size(116, 23);
            txtApiPort.TabIndex = 11;
            // 
            // txtAllowedOrigins
            // 
            txtAllowedOrigins.Location = new Point(148, 394);
            txtAllowedOrigins.Multiline = true;
            txtAllowedOrigins.Name = "txtAllowedOrigins";
            txtAllowedOrigins.ScrollBars = ScrollBars.Vertical;
            txtAllowedOrigins.Size = new Size(336, 78);
            txtAllowedOrigins.TabIndex = 12;
            // 
            // txtTokenDllPath
            // 
            txtTokenDllPath.FormattingEnabled = true;
            txtTokenDllPath.Location = new Point(148, 478);
            txtTokenDllPath.Name = "txtTokenDllPath";
            txtTokenDllPath.Size = new Size(290, 23);
            txtTokenDllPath.TabIndex = 13;
            // 
            // txtApiKey
            // 
            txtApiKey.Location = new Point(148, 507);
            txtApiKey.Name = "txtApiKey";
            txtApiKey.Size = new Size(336, 23);
            txtApiKey.TabIndex = 14;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(503, 394);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(118, 30);
            btnSaveConfig.TabIndex = 15;
            btnSaveConfig.Text = "Lưu cấu hình";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // btnBrowseDll
            // 
            btnBrowseDll.Location = new Point(444, 477);
            btnBrowseDll.Name = "btnBrowseDll";
            btnBrowseDll.Size = new Size(40, 25);
            btnBrowseDll.TabIndex = 16;
            btnBrowseDll.Text = "...";
            btnBrowseDll.UseVisualStyleBackColor = true;
            btnBrowseDll.Click += btnBrowseDll_Click;
            // 
            // btnAutoDetectDll
            // 
            btnAutoDetectDll.Location = new Point(503, 466);
            btnAutoDetectDll.Name = "btnAutoDetectDll";
            btnAutoDetectDll.Size = new Size(118, 38);
            btnAutoDetectDll.TabIndex = 30;
            btnAutoDetectDll.Text = "Tự động dò DLL";
            btnAutoDetectDll.UseVisualStyleBackColor = true;
            btnAutoDetectDll.Click += btnAutoDetectDll_Click;
            // 
            // btnGenerateApiKey
            // 
            btnGenerateApiKey.Location = new Point(503, 506);
            btnGenerateApiKey.Name = "btnGenerateApiKey";
            btnGenerateApiKey.Size = new Size(118, 25);
            btnGenerateApiKey.TabIndex = 31;
            btnGenerateApiKey.Text = "Tạo API key";
            btnGenerateApiKey.UseVisualStyleBackColor = true;
            btnGenerateApiKey.Click += btnGenerateApiKey_Click;
            // 
            // btnOpenLogs
            // 
            btnOpenLogs.Location = new Point(503, 430);
            btnOpenLogs.Name = "btnOpenLogs";
            btnOpenLogs.Size = new Size(118, 30);
            btnOpenLogs.TabIndex = 17;
            btnOpenLogs.Text = "Mở thư mục log";
            btnOpenLogs.UseVisualStyleBackColor = true;
            btnOpenLogs.Click += btnOpenLogs_Click;
            // 
            // chkOnlyLoadableDll
            // 
            chkOnlyLoadableDll.AutoSize = true;
            chkOnlyLoadableDll.Checked = true;
            chkOnlyLoadableDll.CheckState = CheckState.Checked;
            chkOnlyLoadableDll.Location = new Point(148, 532);
            chkOnlyLoadableDll.Name = "chkOnlyLoadableDll";
            chkOnlyLoadableDll.Size = new Size(149, 19);
            chkOnlyLoadableDll.TabIndex = 32;
            chkOnlyLoadableDll.Text = "Chỉ hiện DLL load được";
            chkOnlyLoadableDll.UseVisualStyleBackColor = true;
            chkOnlyLoadableDll.CheckedChanged += chkOnlyLoadableDll_CheckedChanged;
            // 
            // chkAllowLanClients
            // 
            chkAllowLanClients.AutoSize = true;
            chkAllowLanClients.Location = new Point(303, 532);
            chkAllowLanClients.Name = "chkAllowLanClients";
            chkAllowLanClients.Size = new Size(181, 19);
            chkAllowLanClients.TabIndex = 34;
            chkAllowLanClients.Text = "Cho phép máy nội bộ gọi API";
            chkAllowLanClients.UseVisualStyleBackColor = true;
            // 
            // txtTestPayload
            // 
            txtTestPayload.Location = new Point(651, 394);
            txtTestPayload.Multiline = true;
            txtTestPayload.Name = "txtTestPayload";
            txtTestPayload.ScrollBars = ScrollBars.Vertical;
            txtTestPayload.Size = new Size(315, 78);
            txtTestPayload.TabIndex = 18;
            txtTestPayload.Text = "xin chào";
            // 
            // btnSignTest
            // 
            btnSignTest.Location = new Point(651, 478);
            btnSignTest.Name = "btnSignTest";
            btnSignTest.Size = new Size(315, 30);
            btnSignTest.TabIndex = 19;
            btnSignTest.Text = "Ký thử dữ liệu";
            btnSignTest.UseVisualStyleBackColor = true;
            btnSignTest.Click += btnSignTest_Click;
            // 
            // txtSignatureResult
            // 
            txtSignatureResult.Location = new Point(651, 528);
            txtSignatureResult.Multiline = true;
            txtSignatureResult.Name = "txtSignatureResult";
            txtSignatureResult.ReadOnly = true;
            txtSignatureResult.ScrollBars = ScrollBars.Vertical;
            txtSignatureResult.Size = new Size(315, 17);
            txtSignatureResult.TabIndex = 20;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = trayMenuStrip;
            notifyIcon1.Text = "sohoa-sign-pdf";
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            // 
            // trayMenuStrip
            // 
            trayMenuStrip.Items.AddRange(new ToolStripItem[] { menuOpenDashboard, menuRestartService, menuExit });
            trayMenuStrip.Name = "trayMenuStrip";
            trayMenuStrip.Size = new Size(187, 70);
            // 
            // menuOpenDashboard
            // 
            menuOpenDashboard.Name = "menuOpenDashboard";
            menuOpenDashboard.Size = new Size(186, 22);
            menuOpenDashboard.Text = "Mở bảng điều khiển";
            menuOpenDashboard.Click += menuOpenDashboard_Click;
            // 
            // menuRestartService
            // 
            menuRestartService.Name = "menuRestartService";
            menuRestartService.Size = new Size(186, 22);
            menuRestartService.Text = "Khởi động lại dịch vụ";
            menuRestartService.Click += menuRestartService_Click;
            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.Size = new Size(186, 22);
            menuExit.Text = "Thoát";
            menuExit.Click += menuExit_Click;
            // 
            // lblConfigTitle
            // 
            lblConfigTitle.AutoSize = true;
            lblConfigTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblConfigTitle.Location = new Point(20, 336);
            lblConfigTitle.Name = "lblConfigTitle";
            lblConfigTitle.Size = new Size(94, 15);
            lblConfigTitle.TabIndex = 21;
            lblConfigTitle.Text = "Cấu hình cơ bản";
            // 
            // lblCertTitle
            // 
            lblCertTitle.AutoSize = true;
            lblCertTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCertTitle.Location = new Point(20, 129);
            lblCertTitle.Name = "lblCertTitle";
            lblCertTitle.Size = new Size(136, 15);
            lblCertTitle.TabIndex = 22;
            lblCertTitle.Text = "Chứng thư trong token";
            // 
            // lblPin
            // 
            lblPin.AutoSize = true;
            lblPin.Location = new Point(762, 156);
            lblPin.Name = "lblPin";
            lblPin.Size = new Size(26, 15);
            lblPin.TabIndex = 23;
            lblPin.Text = "PIN";
            // 
            // lblApiPortTitle
            // 
            lblApiPortTitle.AutoSize = true;
            lblApiPortTitle.Location = new Point(20, 368);
            lblApiPortTitle.Name = "lblApiPortTitle";
            lblApiPortTitle.Size = new Size(57, 15);
            lblApiPortTitle.TabIndex = 24;
            lblApiPortTitle.Text = "Cổng API";
            // 
            // lblOriginsTitle
            // 
            lblOriginsTitle.AutoSize = true;
            lblOriginsTitle.Location = new Point(20, 397);
            lblOriginsTitle.Name = "lblOriginsTitle";
            lblOriginsTitle.Size = new Size(103, 15);
            lblOriginsTitle.TabIndex = 25;
            lblOriginsTitle.Text = "Địa chỉ được phép";
            // 
            // lblTokenDllTitle
            // 
            lblTokenDllTitle.AutoSize = true;
            lblTokenDllTitle.Location = new Point(20, 481);
            lblTokenDllTitle.Name = "lblTokenDllTitle";
            lblTokenDllTitle.Size = new Size(76, 15);
            lblTokenDllTitle.TabIndex = 26;
            lblTokenDllTitle.Text = "PKCS#11 DLL";
            // 
            // lblApiKeyTitle
            // 
            lblApiKeyTitle.AutoSize = true;
            lblApiKeyTitle.Location = new Point(20, 510);
            lblApiKeyTitle.Name = "lblApiKeyTitle";
            lblApiKeyTitle.Size = new Size(46, 15);
            lblApiKeyTitle.TabIndex = 27;
            lblApiKeyTitle.Text = "API key";
            // 
            // lblPayloadTitle
            // 
            lblPayloadTitle.AutoSize = true;
            lblPayloadTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPayloadTitle.Location = new Point(651, 371);
            lblPayloadTitle.Name = "lblPayloadTitle";
            lblPayloadTitle.Size = new Size(91, 15);
            lblPayloadTitle.TabIndex = 28;
            lblPayloadTitle.Text = "Ký nhanh (test)";
            // 
            // lblSignatureTitle
            // 
            lblSignatureTitle.AutoSize = true;
            lblSignatureTitle.Location = new Point(651, 510);
            lblSignatureTitle.Name = "lblSignatureTitle";
            lblSignatureTitle.Size = new Size(91, 15);
            lblSignatureTitle.TabIndex = 29;
            lblSignatureTitle.Text = "Chữ ký (Base64)";
            // 
            // grpQuickGuide
            // 
            grpQuickGuide.Controls.Add(lblQuickGuide);
            grpQuickGuide.Location = new Point(773, 12);
            grpQuickGuide.Name = "grpQuickGuide";
            grpQuickGuide.Size = new Size(204, 84);
            grpQuickGuide.TabIndex = 33;
            grpQuickGuide.TabStop = false;
            grpQuickGuide.Text = "Hướng dẫn nhanh";
            // 
            // lblQuickGuide
            // 
            lblQuickGuide.Dock = DockStyle.Fill;
            lblQuickGuide.Location = new Point(3, 19);
            lblQuickGuide.Name = "lblQuickGuide";
            lblQuickGuide.Size = new Size(198, 62);
            lblQuickGuide.TabIndex = 0;
            lblQuickGuide.Text = "1) Tự động dò/chọn DLL\r\n2) Lưu cấu hình\r\n3) Nhập PIN và đăng nhập\r\n4) Làm mới + ký thử";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(989, 726);
            Controls.Add(chkAllowLanClients);
            Controls.Add(grpQuickGuide);
            Controls.Add(chkOnlyLoadableDll);
            Controls.Add(btnGenerateApiKey);
            Controls.Add(btnAutoDetectDll);
            Controls.Add(lblSignatureTitle);
            Controls.Add(lblPayloadTitle);
            Controls.Add(lblApiKeyTitle);
            Controls.Add(lblTokenDllTitle);
            Controls.Add(lblOriginsTitle);
            Controls.Add(lblApiPortTitle);
            Controls.Add(lblPin);
            Controls.Add(lblCertTitle);
            Controls.Add(lblConfigTitle);
            Controls.Add(txtSignatureResult);
            Controls.Add(btnSignTest);
            Controls.Add(txtTestPayload);
            Controls.Add(btnOpenLogs);
            Controls.Add(btnBrowseDll);
            Controls.Add(btnSaveConfig);
            Controls.Add(txtApiKey);
            Controls.Add(txtTokenDllPath);
            Controls.Add(txtAllowedOrigins);
            Controls.Add(txtApiPort);
            Controls.Add(txtLogs);
            Controls.Add(chkAutoStart);
            Controls.Add(btnRestartApi);
            Controls.Add(btnRefresh);
            Controls.Add(btnLogin);
            Controls.Add(txtPin);
            Controls.Add(lvCertificates);
            Controls.Add(lblTokenInfo);
            Controls.Add(lblLoginStatus);
            Controls.Add(lblTokenStatus);
            Controls.Add(lblApiStatus);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1005, 765);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "sohoa-sign-pdf | Công cụ ký số cục bộ";
            trayMenuStrip.ResumeLayout(false);
            grpQuickGuide.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
