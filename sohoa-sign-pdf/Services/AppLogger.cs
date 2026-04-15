using System.Collections.Concurrent;

namespace sohoa_sign_pdf.Services;

public sealed class AppLogger
{
    private readonly object _fileLock = new();
    private readonly string _logDirectory;

    public event Action<string>? LogReceived;

    public AppLogger()
    {
        _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(_logDirectory);
    }

    public void Info(string message) => Write("INFO", message);
    public void Warning(string message) => Write("WARN", message);
    public void Error(string message, Exception? exception = null)
    {
        var finalMessage = exception is null ? message : $"{message}{Environment.NewLine}{exception}";
        Write("ERROR", finalMessage);
    }

    private void Write(string level, string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (_fileLock)
        {
            var path = Path.Combine(_logDirectory, $"app-{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(path, line + Environment.NewLine);
        }

        LogReceived?.Invoke(line);
    }
}
