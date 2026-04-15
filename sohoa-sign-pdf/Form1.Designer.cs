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
        private TextBox txtTokenDllPath;
        private TextBox txtApiKey;
        private Button btnSaveConfig;
        private Button btnBrowseDll;
        private Button btnOpenLogs;
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
            txtTokenDllPath = new TextBox();
            txtApiKey = new TextBox();
            btnSaveConfig = new Button();
            btnBrowseDll = new Button();
            btnOpenLogs = new Button();
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
            trayMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // lblApiStatus
            // 
            lblApiStatus.AutoSize = true;
            lblApiStatus.Location = new Point(20, 20);
            lblApiStatus.Name = "lblApiStatus";
            lblApiStatus.Size = new Size(95, 15);
            lblApiStatus.TabIndex = 0;
            lblApiStatus.Text = "API: Initializing";
            // 
            // lblTokenStatus
            // 
            lblTokenStatus.AutoSize = true;
            lblTokenStatus.Location = new Point(20, 45);
            lblTokenStatus.Name = "lblTokenStatus";
            lblTokenStatus.Size = new Size(111, 15);
            lblTokenStatus.TabIndex = 1;
            lblTokenStatus.Text = "Token: Đang kiểm tra";
            // 
            // lblLoginStatus
            // 
            lblLoginStatus.AutoSize = true;
            lblLoginStatus.Location = new Point(20, 70);
            lblLoginStatus.Name = "lblLoginStatus";
            lblLoginStatus.Size = new Size(100, 15);
            lblLoginStatus.TabIndex = 2;
            lblLoginStatus.Text = "Login: Chưa login";
            // 
            // lblTokenInfo
            // 
            lblTokenInfo.AutoSize = true;
            lblTokenInfo.Location = new Point(20, 95);
            lblTokenInfo.Name = "lblTokenInfo";
            lblTokenInfo.Size = new Size(119, 15);
            lblTokenInfo.TabIndex = 3;
            lblTokenInfo.Text = "Label: N/A | Serial: N/A";
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
            colSubject.Text = "Subject";
            colSubject.Width = 240;
            // 
            // colSerial
            // 
            colSerial.Text = "Serial";
            colSerial.Width = 150;
            // 
            // colExpiry
            // 
            colExpiry.Text = "Expiry";
            colExpiry.Width = 120;
            // 
            // colCertId
            // 
            colCertId.Text = "Cert Id";
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
            btnLogin.Size = new Size(98, 30);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "Login Token";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(866, 203);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.TabIndex = 7;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnRestartApi
            // 
            btnRestartApi.Location = new Point(762, 239);
            btnRestartApi.Name = "btnRestartApi";
            btnRestartApi.Size = new Size(204, 30);
            btnRestartApi.TabIndex = 8;
            btnRestartApi.Text = "Restart Local API";
            btnRestartApi.UseVisualStyleBackColor = true;
            btnRestartApi.Click += btnRestartApi_Click;
            // 
            // chkAutoStart
            // 
            chkAutoStart.AutoSize = true;
            chkAutoStart.Location = new Point(762, 286);
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Size = new Size(127, 19);
            chkAutoStart.TabIndex = 9;
            chkAutoStart.Text = "Auto start Windows";
            chkAutoStart.UseVisualStyleBackColor = true;
            chkAutoStart.CheckedChanged += chkAutoStart_CheckedChanged;
            // 
            // txtLogs
            // 
            txtLogs.Location = new Point(20, 551);
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
            btnSaveConfig.Text = "Save Config";
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
            // btnOpenLogs
            // 
            btnOpenLogs.Location = new Point(503, 430);
            btnOpenLogs.Name = "btnOpenLogs";
            btnOpenLogs.Size = new Size(118, 30);
            btnOpenLogs.TabIndex = 17;
            btnOpenLogs.Text = "Open Logs";
            btnOpenLogs.UseVisualStyleBackColor = true;
            btnOpenLogs.Click += btnOpenLogs_Click;
            // 
            // txtTestPayload
            // 
            txtTestPayload.Location = new Point(651, 394);
            txtTestPayload.Multiline = true;
            txtTestPayload.Name = "txtTestPayload";
            txtTestPayload.ScrollBars = ScrollBars.Vertical;
            txtTestPayload.Size = new Size(315, 78);
            txtTestPayload.TabIndex = 18;
            txtTestPayload.Text = "hello signing";
            // 
            // btnSignTest
            // 
            btnSignTest.Location = new Point(651, 478);
            btnSignTest.Name = "btnSignTest";
            btnSignTest.Size = new Size(315, 30);
            btnSignTest.TabIndex = 19;
            btnSignTest.Text = "Sign Test Payload";
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
            trayMenuStrip.Size = new Size(172, 70);
            // 
            // menuOpenDashboard
            // 
            menuOpenDashboard.Name = "menuOpenDashboard";
            menuOpenDashboard.Size = new Size(171, 22);
            menuOpenDashboard.Text = "Open dashboard";
            menuOpenDashboard.Click += menuOpenDashboard_Click;
            // 
            // menuRestartService
            // 
            menuRestartService.Name = "menuRestartService";
            menuRestartService.Size = new Size(171, 22);
            menuRestartService.Text = "Restart service";
            menuRestartService.Click += menuRestartService_Click;
            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.Size = new Size(171, 22);
            menuExit.Text = "Exit";
            menuExit.Click += menuExit_Click;
            // 
            // lblConfigTitle
            // 
            lblConfigTitle.AutoSize = true;
            lblConfigTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblConfigTitle.Location = new Point(20, 336);
            lblConfigTitle.Name = "lblConfigTitle";
            lblConfigTitle.Size = new Size(70, 15);
            lblConfigTitle.TabIndex = 21;
            lblConfigTitle.Text = "Configuration";
            // 
            // lblCertTitle
            // 
            lblCertTitle.AutoSize = true;
            lblCertTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCertTitle.Location = new Point(20, 129);
            lblCertTitle.Name = "lblCertTitle";
            lblCertTitle.Size = new Size(118, 15);
            lblCertTitle.TabIndex = 22;
            lblCertTitle.Text = "Certificates in Token";
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
            lblApiPortTitle.Size = new Size(52, 15);
            lblApiPortTitle.TabIndex = 24;
            lblApiPortTitle.Text = "API Port";
            // 
            // lblOriginsTitle
            // 
            lblOriginsTitle.AutoSize = true;
            lblOriginsTitle.Location = new Point(20, 397);
            lblOriginsTitle.Name = "lblOriginsTitle";
            lblOriginsTitle.Size = new Size(87, 15);
            lblOriginsTitle.TabIndex = 25;
            lblOriginsTitle.Text = "Allowed Origins";
            // 
            // lblTokenDllTitle
            // 
            lblTokenDllTitle.AutoSize = true;
            lblTokenDllTitle.Location = new Point(20, 481);
            lblTokenDllTitle.Name = "lblTokenDllTitle";
            lblTokenDllTitle.Size = new Size(87, 15);
            lblTokenDllTitle.TabIndex = 26;
            lblTokenDllTitle.Text = "PKCS#11 DLL";
            // 
            // lblApiKeyTitle
            // 
            lblApiKeyTitle.AutoSize = true;
            lblApiKeyTitle.Location = new Point(20, 510);
            lblApiKeyTitle.Name = "lblApiKeyTitle";
            lblApiKeyTitle.Size = new Size(47, 15);
            lblApiKeyTitle.TabIndex = 27;
            lblApiKeyTitle.Text = "API Key";
            // 
            // lblPayloadTitle
            // 
            lblPayloadTitle.AutoSize = true;
            lblPayloadTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPayloadTitle.Location = new Point(651, 371);
            lblPayloadTitle.Name = "lblPayloadTitle";
            lblPayloadTitle.Size = new Size(75, 15);
            lblPayloadTitle.TabIndex = 28;
            lblPayloadTitle.Text = "Quick Sign UI";
            // 
            // lblSignatureTitle
            // 
            lblSignatureTitle.AutoSize = true;
            lblSignatureTitle.Location = new Point(651, 510);
            lblSignatureTitle.Name = "lblSignatureTitle";
            lblSignatureTitle.Size = new Size(95, 15);
            lblSignatureTitle.TabIndex = 29;
            lblSignatureTitle.Text = "Signature Base64";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(989, 726);
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
            MinimumSize = new Size(1005, 765);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "sohoa-sign-pdf | Local signer dashboard";
            trayMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
