using sohoa_sign_pdf.Configuration;
using sohoa_sign_pdf.Services;

namespace sohoa_sign_pdf
{
    internal static class Program
    {
        private static Mutex? _singleInstanceMutex;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var createdNew = false;
            _singleInstanceMutex = new Mutex(true, "sohoa-sign-pdf-single-instance", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("?ng d?ng ?ang ch?y r?i.", "sohoa-sign-pdf", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.ThreadException += (_, args) => MessageBox.Show(args.Exception.Message, "L?i ?ng d?ng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppDomain.CurrentDomain.UnhandledException += (_, args) => MessageBox.Show(args.ExceptionObject.ToString(), "L?i nghiêm tr?ng", MessageBoxButtons.OK, MessageBoxIcon.Error);

            var configurationService = new ConfigurationService();
            var logger = new AppLogger();
            var tokenService = new TokenService(configurationService, logger);
            var signingQueueService = new SigningQueueService(tokenService);
            var autoStartService = new AutoStartService("sohoa-sign-pdf");
            var localApiServer = new LocalApiServer(configurationService, tokenService, signingQueueService, logger);
            var dllDiscoveryService = new Pkcs11DllDiscoveryService();

            Application.Run(new Form1(configurationService, logger, tokenService, signingQueueService, localApiServer, autoStartService, dllDiscoveryService));

            tokenService.Dispose();
            _singleInstanceMutex.Dispose();
        }
    }
}