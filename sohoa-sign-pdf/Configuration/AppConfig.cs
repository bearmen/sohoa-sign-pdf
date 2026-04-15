using System.Text.Json;
using System.Text.Json.Serialization;

namespace sohoa_sign_pdf.Configuration;

public sealed class AppConfig
{
    public const string FileName = "appsettings.local.json";

    public int ApiPort { get; set; } = 5005;
    public string ApiKey { get; set; } = "change-me-local-api-key";
    public string[] AllowedOrigins { get; set; } = ["http://localhost:3000", "https://localhost:3000"];
    public string TokenLibraryPath { get; set; } = string.Empty;
    public bool AutoStart { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 15;
    public int RateLimitPerMinute { get; set; } = 30;
    public string LogLevel { get; set; } = "Info";
    public bool DebugMode { get; set; }
}

public sealed class ConfigurationService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ConfigPath { get; }
    public AppConfig Current { get; private set; }

    public ConfigurationService()
    {
        ConfigPath = Path.Combine(AppContext.BaseDirectory, AppConfig.FileName);
        Current = Load();
    }

    public AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            var config = new AppConfig();
            Save(config);
            return config;
        }

        var json = File.ReadAllText(ConfigPath);
        var loaded = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? new AppConfig();
        Current = loaded;
        return loaded;
    }

    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, _jsonOptions));
        Current = config;
    }
}
