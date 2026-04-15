namespace sohoa_sign_pdf.Models;

public sealed class SignRequest
{
    public string Data { get; set; } = string.Empty;
    public string Type { get; set; } = "raw";
    public string? CertId { get; set; }
    public string? Pin { get; set; }
    public string HashAlgorithm { get; set; } = "SHA256";
    public bool DataIsBase64 { get; set; }
}

public sealed class SignHashRequest
{
    public string HashBase64 { get; set; } = string.Empty;
    public string? CertId { get; set; }
    public string? Pin { get; set; }
    public string HashAlgorithm { get; set; } = "SHA256";
}

public sealed class BatchSignRequest
{
    public List<SignRequest> Items { get; set; } = [];
}

public sealed class VerifyRequest
{
    public string Data { get; set; } = string.Empty;
    public string SignatureBase64 { get; set; } = string.Empty;
    public string? CertId { get; set; }
    public bool DataIsBase64 { get; set; }
    public string HashAlgorithm { get; set; } = "SHA256";
}

public sealed class VerifyResult
{
    public bool IsValid { get; set; }
    public bool ChainValid { get; set; }
    public string? Error { get; set; }
}
