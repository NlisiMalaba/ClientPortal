using Shared;
using System.Net;

namespace Domain;

public sealed class RefreshToken : ValueObject
{
    public string TokenHash { get; }

    public DateTime ExpiresAt { get; }

    public string CreatedByIp { get; }

    public DateTime? RevokedAt { get; }

    public string? ReplacedByToken { get; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsActive => !IsRevoked && !IsExpired;

    public RefreshToken(
        string tokenHash,
        DateTime expiresAt,
        string createdByIp,
        DateTime? revokedAt = null,
        string? replacedByToken = null)
    {
        TokenHash = Guard.NotEmpty(tokenHash, nameof(tokenHash)).Trim();
        ExpiresAt = expiresAt;
        CreatedByIp = NormalizeIpAddress(createdByIp, nameof(createdByIp));
        RevokedAt = revokedAt;
        ReplacedByToken = string.IsNullOrWhiteSpace(replacedByToken) ? null : replacedByToken.Trim();

        if (ExpiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException("Refresh token expiration must be in the future.", nameof(expiresAt));
        }

        if (RevokedAt.HasValue && RevokedAt.Value > DateTime.UtcNow)
        {
            throw new ArgumentException("Revocation date cannot be in the future.", nameof(revokedAt));
        }
    }

    public RefreshToken Revoke(DateTime revokedAtUtc, string? replacedByToken = null)
    {
        if (revokedAtUtc > DateTime.UtcNow)
        {
            throw new ArgumentException("Revocation date cannot be in the future.", nameof(revokedAtUtc));
        }

        return new RefreshToken(TokenHash, ExpiresAt, CreatedByIp, revokedAtUtc, replacedByToken);
    }

    private static string NormalizeIpAddress(string ipAddress, string paramName)
    {
        string normalizedIpAddress = Guard.NotEmpty(ipAddress, paramName).Trim();
        if (!IPAddress.TryParse(normalizedIpAddress, out _))
        {
            throw new ArgumentException("IP address is invalid.", paramName);
        }

        return normalizedIpAddress;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TokenHash;
        yield return ExpiresAt;
        yield return CreatedByIp;
        yield return RevokedAt;
        yield return ReplacedByToken;
    }
}
