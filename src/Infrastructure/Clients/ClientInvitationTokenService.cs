using System.Security.Cryptography;
using Application.Clients.Abstractions;

namespace Infrastructure.Clients;

public sealed class ClientInvitationTokenService : IClientInvitationTokenService
{
    private const int TokenSizeBytes = 32;
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(7);

    public ClientInvitationTokenIssueResult Issue(DateTime? nowUtc = null)
    {
        DateTime issuedAtUtc = nowUtc?.ToUniversalTime() ?? DateTime.UtcNow;
        string token = GenerateToken();
        string tokenHash = Hash(token);
        DateTime expiresAtUtc = issuedAtUtc.Add(TokenLifetime);

        return new ClientInvitationTokenIssueResult(token, tokenHash, expiresAtUtc);
    }

    public string Hash(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be empty.", nameof(token));
        }

        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token.Trim());
        byte[] hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes);
    }

    private static string GenerateToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);
        return Convert.ToBase64String(tokenBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
