using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using sohoa_sign_pdf.Configuration;
using sohoa_sign_pdf.Models;

namespace sohoa_sign_pdf.Services;

public sealed class LocalApiServer : IAsyncDisposable
{
    private readonly ConfigurationService _configurationService;
    private readonly TokenService _tokenService;
    private readonly SigningQueueService _signingQueueService;
    private readonly AppLogger _logger;
    private readonly ConcurrentDictionary<string, (DateTime Window, int Count)> _requestTracker = new();

    private WebApplication? _app;

    public bool IsRunning => _app is not null;

    public LocalApiServer(ConfigurationService configurationService, TokenService tokenService, SigningQueueService signingQueueService, AppLogger logger)
    {
        _configurationService = configurationService;
        _tokenService = tokenService;
        _signingQueueService = signingQueueService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_app is not null)
        {
            return;
        }

        var config = _configurationService.Current;
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(LocalApiServer).Assembly.FullName,
            ContentRootPath = AppContext.BaseDirectory,
            EnvironmentName = "Production"
        });

        builder.WebHost.UseUrls($"http://127.0.0.1:{config.ApiPort}");
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.WriteIndented = true;
        });

        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            if (!IPAddress.IsLoopback(context.Connection.RemoteIpAddress ?? IPAddress.None))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Chỉ cho phép gửi từ localhost." }, cancellationToken);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/health"))
            {
                await next();
                return;
            }

            if (!IsOriginAllowed(context.Request.Headers.Origin))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Origin không hợp lệ." }, cancellationToken);
                return;
            }

            if (!IsApiKeyValid(context.Request.Headers["X-Api-Key"]))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Thiếu hoặc sai API key." }, cancellationToken);
                return;
            }

            if (!PassRateLimit(context.Connection.RemoteIpAddress?.ToString() ?? "local"))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded." }, cancellationToken);
                return;
            }

            await next();
        });

        app.MapGet("/health", () => Results.Ok(new
        {
            status = "ok",
            apiPort = config.ApiPort,
            version = Application.ProductVersion
        }));

        app.MapGet("/token/status", async (CancellationToken ct) =>
        {
            var status = await _tokenService.GetStatusAsync(ct);
            return Results.Ok(status);
        });

        app.MapGet("/certificates", async (CancellationToken ct) =>
        {
            var certificates = await _tokenService.GetCertificatesAsync(ct);
            return Results.Ok(certificates.Select(x => new
            {
                x.Id,
                x.Label,
                x.Subject,
                x.SerialNumber,
                x.Thumbprint,
                x.NotBefore,
                x.NotAfter,
                x.IsExpired
            }));
        });

        app.MapPost("/sign", async (SignRequest request, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Data))
            {
                return Results.BadRequest(new { error = "Thiếu data." });
            }

            if (!IsSupportedType(request.Type))
            {
                return Results.BadRequest(new { error = "Chưa hỗ trợ raw/json/base64 bên MVP." });
            }

            var normalized = NormalizeRequest(request);
            var result = await _signingQueueService.SignDataAsync(normalized, ct);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        app.MapPost("/sign-hash", async (SignHashRequest request, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.HashBase64))
            {
                return Results.BadRequest(new { error = "Thiếu hashBase64." });
            }

            var result = await _signingQueueService.SignHashAsync(request, ct);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        app.MapPost("/sign-batch", async (BatchSignRequest request, CancellationToken ct) =>
        {
            var results = new List<SignResult>();
            foreach (var item in request.Items)
            {
                results.Add(await _signingQueueService.SignDataAsync(NormalizeRequest(item), ct));
            }

            return Results.Ok(results);
        });

        app.MapGet("/get-public-key", async (string? certId, CancellationToken ct) =>
        {
            var publicKey = await _tokenService.GetPublicKeyAsync(certId, ct);
            return publicKey is null
                ? Results.NotFound(new { error = "Không tìm thấy public key." })
                : Results.Ok(new { publicKeyBase64 = Convert.ToBase64String(publicKey) });
        });

        app.MapPost("/verify", async (VerifyRequest request, CancellationToken ct) =>
        {
            var result = await _tokenService.VerifyAsync(request.Data, request.SignatureBase64, request.CertId, request.DataIsBase64, request.HashAlgorithm, ct);
            return result.Error is null ? Results.Ok(result) : Results.BadRequest(result);
        });

        await app.StartAsync(cancellationToken);
        _app = app;
        _logger.Info($"Local API đã chạy tại http://127.0.0.1:{config.ApiPort}");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync(cancellationToken);
        await _app.DisposeAsync();
        _app = null;
        _logger.Warning("Local API đã dừng.");
    }

    private bool IsOriginAllowed(string? origin)
    {
        var allowed = _configurationService.Current.AllowedOrigins ?? [];
        if (allowed.Length == 0 || string.IsNullOrWhiteSpace(origin))
        {
            return true;
        }

        return allowed.Any(x => string.Equals(x, origin, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsApiKeyValid(string? apiKey)
    {
        return !string.IsNullOrWhiteSpace(apiKey)
            && apiKey.Equals(_configurationService.Current.ApiKey, StringComparison.Ordinal);
    }

    private bool PassRateLimit(string clientKey)
    {
        var now = DateTime.UtcNow;
        var limit = Math.Max(1, _configurationService.Current.RateLimitPerMinute);

        var current = _requestTracker.AddOrUpdate(clientKey,
            _ => (now, 1),
            (_, existing) => now - existing.Window > TimeSpan.FromMinutes(1)
                ? (now, 1)
                : (existing.Window, existing.Count + 1));

        return current.Count <= limit;
    }

    private static bool IsSupportedType(string type)
    {
        return type.Equals("raw", StringComparison.OrdinalIgnoreCase)
            || type.Equals("json", StringComparison.OrdinalIgnoreCase)
            || type.Equals("base64", StringComparison.OrdinalIgnoreCase);
    }

    private static SignRequest NormalizeRequest(SignRequest request)
    {
        if (request.Type.Equals("base64", StringComparison.OrdinalIgnoreCase))
        {
            request.DataIsBase64 = true;
        }

        return request;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}