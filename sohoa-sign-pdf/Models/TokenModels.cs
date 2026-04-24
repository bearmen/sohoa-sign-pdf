using System.Security.Cryptography.X509Certificates;

namespace sohoa_sign_pdf.Models;

public enum TokenHealthState
{
    Unknown,
    Ready,
    TokenMissing,
    LoginRequired,
    Error
}

public sealed class TokenStatusInfo
{
    public bool IsLibraryConfigured { get; set; }
    public bool IsTokenPresent { get; set; }
    public bool IsLoggedIn { get; set; }
    public string? SlotDescription { get; set; }
    public string? TokenLabel { get; set; }
    public string? SerialNumber { get; set; }
    public string? Message { get; set; }
    public TokenHealthState State { get; set; }
}

public sealed class CertificateInfo
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Thumbprint { get; set; } = string.Empty;
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
    public bool IsExpired => DateTime.UtcNow > NotAfter.ToUniversalTime();
    public byte[] RawData { get; set; } = [];

    public X509Certificate2 ToCertificate() => new(RawData);
}

public sealed class SignResult
{
    public bool Success { get; set; }
    public string? SignatureBase64 { get; set; }
    public string? Algorithm { get; set; }
    public string? CertificateId { get; set; }
    public string? SignatureFormat { get; set; }
    public string? Error { get; set; }
}
