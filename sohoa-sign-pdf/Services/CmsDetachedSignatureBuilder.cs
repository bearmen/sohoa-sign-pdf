using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using CmsAttribute = Org.BouncyCastle.Asn1.Cms.Attribute;
using CmsAttributes = Org.BouncyCastle.Asn1.Cms.CmsAttributes;
using CmsContentInfo = Org.BouncyCastle.Asn1.Cms.ContentInfo;
using CmsIssuerAndSerialNumber = Org.BouncyCastle.Asn1.Cms.IssuerAndSerialNumber;
using CmsObjectIdentifiers = Org.BouncyCastle.Asn1.Cms.CmsObjectIdentifiers;
using CmsSignedData = Org.BouncyCastle.Asn1.Cms.SignedData;
using CmsSignerIdentifier = Org.BouncyCastle.Asn1.Cms.SignerIdentifier;
using CmsSignerInfo = Org.BouncyCastle.Asn1.Cms.SignerInfo;
using CmsTime = Org.BouncyCastle.Asn1.Cms.Time;
using CmsAttributesSet = Org.BouncyCastle.Asn1.Cms.Attributes;

namespace sohoa_sign_pdf.Services;

internal static class CmsDetachedSignatureBuilder
{
    public static byte[] BuildDetached(byte[] contentDigest, X509Certificate2 certificate, Func<byte[], HashAlgorithmName, byte[]> signDigest, string hashAlgorithm)
    {
        var digestAlgorithm = CreateDigestAlgorithmIdentifier(hashAlgorithm);
        var signatureAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance);
        var bcCertificate = DotNetUtilities.FromX509Certificate(certificate);

        var signedAttributes = CreateSignedAttributes(contentDigest);
        var signedAttributesBytes = signedAttributes.GetDerEncoded();
        var signedAttributesDigest = ComputeHash(signedAttributesBytes, hashAlgorithm);
        var signature = signDigest(signedAttributesDigest, ResolveHashName(hashAlgorithm));

        var signerInfo = new CmsSignerInfo(
            new CmsSignerIdentifier(new CmsIssuerAndSerialNumber(bcCertificate.IssuerDN, bcCertificate.SerialNumber)),
            digestAlgorithm,
            signedAttributes,
            signatureAlgorithm,
            new DerOctetString(signature),
            null);

        var signedData = new CmsSignedData(
            new DerSet(digestAlgorithm),
            new CmsContentInfo(PkcsObjectIdentifiers.Data, null),
            new DerSet(Asn1Object.FromByteArray(certificate.RawData)),
            null,
            new DerSet(signerInfo));

        var cms = new CmsContentInfo(CmsObjectIdentifiers.SignedData, signedData);
        return cms.GetDerEncoded();
    }

    private static CmsAttributesSet CreateSignedAttributes(byte[] contentDigest)
    {
        var attributes = new Asn1EncodableVector
        {
            new CmsAttribute(CmsAttributes.ContentType, new DerSet(PkcsObjectIdentifiers.Data)),
            new CmsAttribute(CmsAttributes.MessageDigest, new DerSet(new DerOctetString(contentDigest))),
            new CmsAttribute(CmsAttributes.SigningTime, new DerSet(new CmsTime(DateTime.UtcNow)))
        };

        return new CmsAttributesSet(attributes);
    }

    private static AlgorithmIdentifier CreateDigestAlgorithmIdentifier(string hashAlgorithm) => new(new DerObjectIdentifier(GetDigestOid(hashAlgorithm)), DerNull.Instance);

    private static string GetDigestOid(string hashAlgorithm) => hashAlgorithm.ToUpperInvariant() switch
    {
        "SHA1" or "SHA-1" => "1.3.14.3.2.26",
        _ => "2.16.840.1.101.3.4.2.1"
    };

    private static HashAlgorithmName ResolveHashName(string hashAlgorithm) => hashAlgorithm.ToUpperInvariant() switch
    {
        "SHA1" or "SHA-1" => HashAlgorithmName.SHA1,
        _ => HashAlgorithmName.SHA256
    };

    private static byte[] ComputeHash(byte[] data, string hashAlgorithm) => hashAlgorithm.ToUpperInvariant() switch
    {
        "SHA1" or "SHA-1" => SHA1.HashData(data),
        _ => SHA256.HashData(data)
    };
}
