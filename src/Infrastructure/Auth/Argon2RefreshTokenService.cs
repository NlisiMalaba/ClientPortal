using Application.Auth.Abstractions;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Auth;

public sealed class Argon2RefreshTokenService : IRefreshTokenService
{
    private readonly RefreshTokenOptions _options;
    private readonly byte[] _pepperBytes;

    public Argon2RefreshTokenService(IOptions<RefreshTokenOptions> options)
    {
        _options = options.Value;
        ValidateOptions(_options);
        _pepperBytes = Encoding.UTF8.GetBytes(_options.Pepper);
    }

    public RefreshTokenIssueResult Generate(string createdByIp, DateTime? nowUtc = null)
    {
        DateTime issuedAt = nowUtc ?? DateTime.UtcNow;
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(_options.TokenSizeBytes);
        string token = Convert.ToBase64String(tokenBytes);
        string tokenHash = Hash(token);
        DateTime expiresAt = issuedAt.AddDays(_options.LifetimeDays);

        return new RefreshTokenIssueResult(
            Token: token,
            TokenHash: tokenHash,
            ExpiresAt: expiresAt,
            CreatedByIp: createdByIp);
    }

    public string Hash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token cannot be empty.", nameof(refreshToken));
        }

        string normalizedToken = refreshToken.Trim();
        byte[] tokenBytes = Encoding.UTF8.GetBytes(normalizedToken);
        byte[] salt = DeriveDeterministicSalt(tokenBytes);
        byte[] hashBytes = DeriveHash(tokenBytes, salt);
        return Convert.ToBase64String(hashBytes);
    }

    public bool Validate(string refreshToken, string storedTokenHash)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(storedTokenHash))
        {
            return false;
        }

        byte[] expectedHashBytes;
        try
        {
            expectedHashBytes = Convert.FromBase64String(storedTokenHash.Trim());
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] actualHashBytes = Convert.FromBase64String(Hash(refreshToken));
        return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }

    private byte[] DeriveHash(byte[] tokenBytes, byte[] salt)
    {
        Argon2id argon2 = new(tokenBytes)
        {
            Salt = salt,
            Iterations = _options.Iterations,
            MemorySize = _options.MemorySizeKb,
            DegreeOfParallelism = _options.DegreeOfParallelism
        };

        return argon2.GetBytesAsync(32).GetAwaiter().GetResult();
    }

    private byte[] DeriveDeterministicSalt(byte[] tokenBytes)
    {
        using HMACSHA256 hmac = new(_pepperBytes);
        byte[] keyedHash = hmac.ComputeHash(tokenBytes);
        return keyedHash[..16];
    }

    private static void ValidateOptions(RefreshTokenOptions options)
    {
        if (options.TokenSizeBytes < 32)
        {
            throw new InvalidOperationException("RefreshToken:TokenSizeBytes must be at least 32.");
        }

        if (options.LifetimeDays <= 0)
        {
            throw new InvalidOperationException("RefreshToken:LifetimeDays must be greater than zero.");
        }

        if (options.Iterations <= 0 || options.MemorySizeKb <= 0 || options.DegreeOfParallelism <= 0)
        {
            throw new InvalidOperationException("RefreshToken Argon2id settings must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.Pepper))
        {
            throw new InvalidOperationException("RefreshToken:Pepper must be configured.");
        }
    }
}
