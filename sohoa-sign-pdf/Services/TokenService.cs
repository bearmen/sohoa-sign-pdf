using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.Factories;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using sohoa_sign_pdf.Configuration;
using sohoa_sign_pdf.Models;

namespace sohoa_sign_pdf.Services;

public sealed class TokenService : IDisposable
{
    private readonly ConfigurationService _configurationService;
    private readonly AppLogger _logger;
    private readonly Pkcs11InteropFactories _factories = new();
    private readonly SemaphoreSlim _sync = new(1, 1);

    private IPkcs11Library? _library;
    private ISlot? _activeSlot;
    private ISession? _session;
    private DateTime _lastLoginUtc = DateTime.MinValue;
    private bool _isLoggedIn;

    public event Action<TokenStatusInfo>? StatusChanged;

    public TokenService(ConfigurationService configurationService, AppLogger logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<TokenStatusInfo> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            return GetStatusInternal();
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<bool> LoginAsync(string pin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            throw new InvalidOperationException("PIN không được để trống.");
        }

        await _sync.WaitAsync(cancellationToken);
        try
        {
            EnsureLibraryLoaded();
            EnsureActiveSlot();
            EnsureSession();

            if (_isLoggedIn)
            {
                _lastLoginUtc = DateTime.UtcNow;
                return true;
            }

            _session!.Login(CKU.CKU_USER, pin);
            _isLoggedIn = true;
            _lastLoginUtc = DateTime.UtcNow;
            PublishStatus(GetStatusInternal());
            _logger.Info("USB Token login thành công.");
            return true;
        }
        catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_USER_ALREADY_LOGGED_IN)
        {
            _isLoggedIn = true;
            _lastLoginUtc = DateTime.UtcNow;
            return true;
        }
        catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_PIN_INCORRECT)
        {
            _logger.Warning("Sai PIN USB Token.");
            throw new InvalidOperationException("Sai PIN.", ex);
        }
        catch (Exception ex)
        {
            _logger.Error("Login USB Token thất bại.", ex);
            throw;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            _session?.Logout();
        }
        catch (Pkcs11Exception)
        {
        }
        finally
        {
            _isLoggedIn = false;
            _lastLoginUtc = DateTime.MinValue;
            PublishStatus(GetStatusInternal());
            _sync.Release();
        }
    }

    public async Task<IReadOnlyList<CertificateInfo>> GetCertificatesAsync(CancellationToken cancellationToken = default)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            EnsureLibraryLoaded();
            EnsureActiveSlot();
            EnsureSession();
            EnforceSessionTimeout();
            return GetCertificatesInternal();
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task<SignResult> SignDataAsync(string data, string? certId, string? pin, string hashAlgorithm, bool inputIsBase64, CancellationToken cancellationToken = default)
    {
        var bytes = inputIsBase64 ? Convert.FromBase64String(data) : System.Text.Encoding.UTF8.GetBytes(data);
        var hash = ComputeHash(bytes, hashAlgorithm);
        return await SignHashInternalAsync(hash, certId, pin, hashAlgorithm, cancellationToken, "raw");
    }

    public async Task<SignResult> SignHashAsync(string hashBase64, string? certId, string? pin, string hashAlgorithm, CancellationToken cancellationToken = default)
    {
        var hash = Convert.FromBase64String(hashBase64);
        return await SignHashInternalAsync(hash, certId, pin, hashAlgorithm, cancellationToken, "raw");
    }

    public async Task<SignResult> SignCmsDataAsync(string data, string? certId, string? pin, string hashAlgorithm, bool inputIsBase64, CancellationToken cancellationToken = default)
    {
        var bytes = inputIsBase64 ? Convert.FromBase64String(data) : System.Text.Encoding.UTF8.GetBytes(data);
        var hash = ComputeHash(bytes, hashAlgorithm);
        return await SignCmsInternalAsync(hash, certId, pin, hashAlgorithm, cancellationToken);
    }

    public async Task<SignResult> SignCmsHashAsync(string hashBase64, string? certId, string? pin, string hashAlgorithm, CancellationToken cancellationToken = default)
    {
        var hash = Convert.FromBase64String(hashBase64);
        return await SignCmsInternalAsync(hash, certId, pin, hashAlgorithm, cancellationToken);
    }

    public async Task<VerifyResult> VerifyAsync(string data, string signatureBase64, string? certId, bool inputIsBase64, string hashAlgorithm, CancellationToken cancellationToken = default)
    {
        var certificates = await GetCertificatesAsync(cancellationToken);
        var selected = SelectCertificate(certificates, certId);
        if (selected is null)
        {
            return new VerifyResult { Error = "Không tìm thấy chứng thư để verify." };
        }

        var payload = inputIsBase64 ? Convert.FromBase64String(data) : System.Text.Encoding.UTF8.GetBytes(data);
        var signature = Convert.FromBase64String(signatureBase64);
        var certificate = selected.ToCertificate();
        using RSA? rsa = certificate.GetRSAPublicKey();
        if (rsa is null)
        {
            return new VerifyResult { Error = "Certificate không hỗ trợ RSA public key." };
        }

        var hash = ComputeHash(payload, hashAlgorithm);
        var valid = rsa.VerifyHash(hash, signature, ResolveHashName(hashAlgorithm), RSASignaturePadding.Pkcs1);

        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        var chainValid = chain.Build(certificate);

        return new VerifyResult { IsValid = valid, ChainValid = chainValid };
    }

    public async Task<byte[]?> GetPublicKeyAsync(string? certId, CancellationToken cancellationToken = default)
    {
        var certificates = await GetCertificatesAsync(cancellationToken);
        var selected = SelectCertificate(certificates, certId);
        var certificate = selected?.ToCertificate();
        using RSA? rsa = certificate?.GetRSAPublicKey();
        return rsa?.ExportSubjectPublicKeyInfo();
    }

    private async Task<SignResult> SignHashInternalAsync(byte[] hash, string? certId, string? pin, string hashAlgorithm, CancellationToken cancellationToken, string signatureFormat)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            var context = EnsureSigningContext(certId, pin);
            var signature = SignDigestWithPrivateKey(context.KeyHandle, hash, hashAlgorithm);
            _lastLoginUtc = DateTime.UtcNow;
            _logger.Info($"Ký dữ liệu thành công với certificate {context.Certificate.Subject}.");

            PublishStatus(GetStatusInternal());
            return new SignResult
            {
                Success = true,
                SignatureBase64 = Convert.ToBase64String(signature),
                Algorithm = hashAlgorithm,
                CertificateId = context.Certificate.Id,
                SignatureFormat = signatureFormat
            };
        }
        catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_USER_NOT_LOGGED_IN)
        {
            _isLoggedIn = false;
            return new SignResult { Error = "Token chưa login.", SignatureFormat = signatureFormat };
        }
        catch (Exception ex)
        {
            _logger.Error("Ký dữ liệu thất bại.", ex);
            return new SignResult { Error = ex.Message, SignatureFormat = signatureFormat };
        }
        finally
        {
            _sync.Release();
        }
    }

    private async Task<SignResult> SignCmsInternalAsync(byte[] contentHash, string? certId, string? pin, string hashAlgorithm, CancellationToken cancellationToken)
    {
        await _sync.WaitAsync(cancellationToken);
        try
        {
            var context = EnsureSigningContext(certId, pin);
            var certificate = context.Certificate.ToCertificate();
            var cms = CmsDetachedSignatureBuilder.BuildDetached(
                contentHash,
                certificate,
                (digest, algorithm) => SignDigestWithPrivateKey(context.KeyHandle, digest, NormalizeHashAlgorithm(algorithm)),
                hashAlgorithm);

            _lastLoginUtc = DateTime.UtcNow;
            _logger.Info($"Tạo detached CMS thành công với certificate {context.Certificate.Subject}.");
            PublishStatus(GetStatusInternal());

            return new SignResult
            {
                Success = true,
                SignatureBase64 = Convert.ToBase64String(cms),
                Algorithm = hashAlgorithm,
                CertificateId = context.Certificate.Id,
                SignatureFormat = "cms-detached"
            };
        }
        catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_USER_NOT_LOGGED_IN)
        {
            _isLoggedIn = false;
            return new SignResult { Error = "Token chưa login.", SignatureFormat = "cms-detached" };
        }
        catch (Exception ex)
        {
            _logger.Error("Tạo detached CMS thất bại.", ex);
            return new SignResult { Error = ex.Message, SignatureFormat = "cms-detached" };
        }
        finally
        {
            _sync.Release();
        }
    }

    private (CertificateInfo Certificate, IObjectHandle KeyHandle) EnsureSigningContext(string? certId, string? pin)
    {
        EnsureLibraryLoaded();
        EnsureActiveSlot();
        EnsureSession();
        EnforceSessionTimeout();

        if (!_isLoggedIn)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                throw new InvalidOperationException("Token chưa login.");
            }

            _session!.Login(CKU.CKU_USER, pin);
            _isLoggedIn = true;
            _lastLoginUtc = DateTime.UtcNow;
        }

        var certificates = GetCertificatesInternal();
        var selected = SelectCertificate(certificates, certId);
        if (selected is null)
        {
            throw new InvalidOperationException("Không tìm thấy certificate phù hợp.");
        }

        var keyHandle = FindPrivateKeyHandle(selected.Id);
        if (keyHandle is null)
        {
            throw new InvalidOperationException("Không tìm thấy private key trên token.");
        }

        return (selected, keyHandle);
    }

    private byte[] SignDigestWithPrivateKey(IObjectHandle keyHandle, byte[] hash, string hashAlgorithm)
    {
        var mechanism = _factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
        var digestInfo = BuildDigestInfo(hash, hashAlgorithm);
        return _session!.Sign(mechanism, keyHandle, digestInfo);
    }

    private static byte[] BuildDigestInfo(byte[] hash, string hashAlgorithm)
    {
        var algorithmIdentifier = new AlgorithmIdentifier(new DerObjectIdentifier(GetDigestOid(hashAlgorithm)), DerNull.Instance);
        var digestInfo = new DigestInfo(algorithmIdentifier, hash);
        return digestInfo.GetDerEncoded();
    }

    private List<CertificateInfo> GetCertificatesInternal()
    {
        var template = new List<IObjectAttribute>
        {
            _factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
        };

        var handles = _session!.FindAllObjects(template);
        var result = new List<CertificateInfo>();
        foreach (var handle in handles)
        {
            var attributes = _session.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_VALUE,
                CKA.CKA_ID,
                CKA.CKA_LABEL
            });

            var raw = attributes[0].GetValueAsByteArray();
            if (raw is null || raw.Length == 0)
            {
                continue;
            }

            var cert = new X509Certificate2(raw);
            result.Add(new CertificateInfo
            {
                Id = Convert.ToHexString(attributes[1].GetValueAsByteArray() ?? []),
                Label = attributes[2].GetValueAsString() ?? string.Empty,
                Subject = cert.Subject,
                Issuer = cert.Issuer,
                SerialNumber = cert.SerialNumber,
                Thumbprint = cert.Thumbprint,
                CommonName = GetCommonName(cert),
                Organization = GetSubjectAttribute(cert, "O"),
                Email = GetEmail(cert),
                NotBefore = cert.NotBefore,
                NotAfter = cert.NotAfter,
                RawData = raw
            });
        }

        return result
            .OrderByDescending(x => x.NotAfter)
            .ToList();
    }

    private CertificateInfo? SelectCertificate(IReadOnlyList<CertificateInfo> certificates, string? certId)
    {
        if (certificates.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(certId))
        {
            return certificates
                .Where(x => !x.IsExpired)
                .OrderByDescending(x => x.NotAfter)
                .FirstOrDefault() ?? certificates[0];
        }

        return certificates.FirstOrDefault(x =>
            x.Id.Equals(certId, StringComparison.OrdinalIgnoreCase)
            || x.SerialNumber.Equals(certId, StringComparison.OrdinalIgnoreCase)
            || x.Thumbprint.Equals(certId, StringComparison.OrdinalIgnoreCase)
            || x.Subject.Contains(certId, StringComparison.OrdinalIgnoreCase));
    }

    private IObjectHandle? FindPrivateKeyHandle(string certId)
    {
        var keyTemplate = new List<IObjectAttribute>
        {
            _factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            _factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Convert.FromHexString(certId))
        };

        return _session!.FindAllObjects(keyTemplate).FirstOrDefault();
    }

    private TokenStatusInfo GetStatusInternal()
    {
        var config = _configurationService.Current;
        if (string.IsNullOrWhiteSpace(config.TokenLibraryPath) || !File.Exists(config.TokenLibraryPath))
        {
            return new TokenStatusInfo
            {
                IsLibraryConfigured = false,
                State = TokenHealthState.Error,
                Message = "Chưa cấu hình đường dẫn PKCS#11 DLL."
            };
        }

        try
        {
            EnsureLibraryLoaded();
            var slots = _library!.GetSlotList(SlotsType.WithTokenPresent);
            var slot = slots.FirstOrDefault();
            if (slot is null)
            {
                _activeSlot = null;
                _session = null;
                _isLoggedIn = false;
                return new TokenStatusInfo
                {
                    IsLibraryConfigured = true,
                    IsTokenPresent = false,
                    State = TokenHealthState.TokenMissing,
                    Message = "Chưa phát hiện USB Token."
                };
            }

            _activeSlot = slot;
            var slotInfo = slot.GetSlotInfo();
            var tokenInfo = slot.GetTokenInfo();

            return new TokenStatusInfo
            {
                IsLibraryConfigured = true,
                IsTokenPresent = true,
                IsLoggedIn = _isLoggedIn,
                SlotDescription = slotInfo.SlotDescription?.Trim(),
                TokenLabel = tokenInfo.Label?.Trim(),
                SerialNumber = tokenInfo.SerialNumber?.Trim(),
                State = _isLoggedIn ? TokenHealthState.Ready : TokenHealthState.LoginRequired,
                Message = _isLoggedIn ? "Token sẵn sàng." : "Token đã cắm, cần login."
            };
        }
        catch (Exception ex)
        {
            _isLoggedIn = false;
            return new TokenStatusInfo
            {
                IsLibraryConfigured = true,
                State = TokenHealthState.Error,
                Message = ex.Message
            };
        }
    }

    private void EnsureLibraryLoaded()
    {
        var libraryPath = _configurationService.Current.TokenLibraryPath;
        if (string.IsNullOrWhiteSpace(libraryPath) || !File.Exists(libraryPath))
        {
            throw new FileNotFoundException("Không tìm thấy PKCS#11 library.", libraryPath);
        }

        if (_library is not null)
        {
            return;
        }

        _library = _factories.Pkcs11LibraryFactory.LoadPkcs11Library(_factories, libraryPath, AppType.MultiThreaded);
    }

    private void EnsureActiveSlot()
    {
        if (_library is null)
        {
            throw new InvalidOperationException("PKCS#11 library chưa được load.");
        }

        if (_activeSlot is not null)
        {
            try
            {
                _ = _activeSlot.GetTokenInfo();
                return;
            }
            catch
            {
                _activeSlot = null;
                _session = null;
                _isLoggedIn = false;
            }
        }

        _activeSlot = _library.GetSlotList(SlotsType.WithTokenPresent).FirstOrDefault();
        if (_activeSlot is null)
        {
            throw new InvalidOperationException("Không tìm thấy USB Token nào đang cắm.");
        }
    }

    private void EnsureSession()
    {
        if (_session is not null)
        {
            return;
        }

        _session = _activeSlot!.OpenSession(SessionType.ReadWrite);
    }

    private void EnforceSessionTimeout()
    {
        var timeout = TimeSpan.FromMinutes(Math.Max(1, _configurationService.Current.SessionTimeoutMinutes));
        if (!_isLoggedIn || _lastLoginUtc == DateTime.MinValue || DateTime.UtcNow - _lastLoginUtc < timeout)
        {
            return;
        }

        try
        {
            _session?.Logout();
            _logger.Info("Session token đã tự logout do timeout.");
        }
        catch (Pkcs11Exception)
        {
        }
        finally
        {
            _isLoggedIn = false;
            _lastLoginUtc = DateTime.MinValue;
        }
    }

    private void PublishStatus(TokenStatusInfo status)
    {
        StatusChanged?.Invoke(status);
    }

    private static HashAlgorithmName ResolveHashName(string hashAlgorithm) => hashAlgorithm.ToUpperInvariant() switch
    {
        "SHA1" or "SHA-1" => HashAlgorithmName.SHA1,
        _ => HashAlgorithmName.SHA256
    };

    private static string NormalizeHashAlgorithm(HashAlgorithmName hashAlgorithmName)
    {
        if (hashAlgorithmName == HashAlgorithmName.SHA1)
        {
            return "SHA1";
        }

        return "SHA256";
    }

    private static string GetDigestOid(string hashAlgorithm) => hashAlgorithm.ToUpperInvariant() switch
    {
        "SHA1" or "SHA-1" => "1.3.14.3.2.26",
        _ => "2.16.840.1.101.3.4.2.1"
    };

    private static byte[] ComputeHash(byte[] data, string hashAlgorithm) => hashAlgorithm.ToUpperInvariant() switch
    {
        "SHA1" or "SHA-1" => SHA1.HashData(data),
        _ => SHA256.HashData(data)
    };

    public void Dispose()
    {
        try
        {
            _session?.Dispose();
            _library?.Dispose();
        }
        catch
        {
        }
    }

    private static string? GetCommonName(X509Certificate2 cert)
    {
        var commonName = cert.GetNameInfo(X509NameType.SimpleName, false);
        return string.IsNullOrWhiteSpace(commonName)
            ? GetSubjectAttribute(cert, "CN")
            : commonName;
    }

    private static string? GetEmail(X509Certificate2 cert)
    {
        var email = cert.GetNameInfo(X509NameType.EmailName, false);
        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        return GetSubjectAttribute(cert, "E", "EMAILADDRESS");
    }

    private static string? GetSubjectAttribute(X509Certificate2 cert, params string[] keys)
    {
        var subject = cert.SubjectName.Name;
        if (string.IsNullOrWhiteSpace(subject) || keys.Length == 0)
        {
            return null;
        }

        var parts = subject.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var index = part.IndexOf('=');
            if (index <= 0 || index == part.Length - 1)
            {
                continue;
            }

            var key = part[..index].Trim();
            if (!keys.Any(x => key.Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var value = part[(index + 1)..].Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }
}
