using Microsoft.Win32;

namespace sohoa_sign_pdf.Services;

public sealed class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string _appName;

    public AutoStartService(string appName)
    {
        _appName = appName;
    }

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
        return key?.GetValue(_appName) is string;
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true) ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (enabled)
        {
            key.SetValue(_appName, $"\"{Application.ExecutablePath}\"");
            return;
        }

        key.DeleteValue(_appName, false);
    }
}
