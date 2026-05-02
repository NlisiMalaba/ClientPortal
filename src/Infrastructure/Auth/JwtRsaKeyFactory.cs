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

        string trimmed = pemValue.Replace("\\n", "\n").Trim();
        if (trimmed.Contains("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"{optionName} still contains a placeholder. Replace Jwt:PrivateKeyPem and Jwt:PublicKeyPem with PEM text "
                + "(full key body, not a file path). For Docker, set Jwt__PrivateKeyPem and Jwt__PublicKeyPem in .env using \\n "
                + "between lines. Generate keys: openssl genrsa -out private.pem 2048 && openssl rsa -in private.pem -pubout -out public.pem.");
        }

        return trimmed;
    }
}
