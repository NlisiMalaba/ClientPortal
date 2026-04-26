using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Infrastructure.Auth;

public static class JwtRsaKeyFactory
{
    public static RsaSecurityKey CreatePrivateKey(string privateKeyPem)
    {
        string normalizedPem = NormalizePem(privateKeyPem, "Jwt:PrivateKeyPem");
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(normalizedPem);
        return new RsaSecurityKey(rsa);
    }

    public static RsaSecurityKey CreatePublicKey(string publicKeyPem)
    {
        string normalizedPem = NormalizePem(publicKeyPem, "Jwt:PublicKeyPem");
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(normalizedPem);
        return new RsaSecurityKey(rsa);
    }

    private static string NormalizePem(string pemValue, string optionName)
    {
        if (string.IsNullOrWhiteSpace(pemValue))
        {
            throw new InvalidOperationException($"{optionName} must be configured.");
        }

        return pemValue.Replace("\\n", "\n").Trim();
    }
}
