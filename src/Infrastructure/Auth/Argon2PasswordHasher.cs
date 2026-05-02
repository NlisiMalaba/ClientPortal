using Application.Auth.Abstractions;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Auth;

/// <summary>
/// Argon2id password hashing for owner/staff credentials (separate from refresh-token hashing).
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private readonly RefreshTokenOptions _options;
    private readonly byte[] _pepperBytes;

    public Argon2PasswordHasher(IOptions<RefreshTokenOptions> options)
    {
        _options = options.Value;
        ValidateOptions(_options);
        _pepperBytes = Encoding.UTF8.GetBytes(_options.Pepper);
    }

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = DeriveHash(Encoding.UTF8.GetBytes(password.Trim()), salt);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        string[] parts = passwordHash.Trim().Split('.', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
            expectedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password.Trim());
        byte[] actualHash = DeriveHash(passwordBytes, salt);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private byte[] DeriveHash(byte[] passwordBytes, byte[] salt)
    {
        byte[] keyedPassword = CombinePepper(passwordBytes);

        Argon2id argon2 = new(keyedPassword)
        {
            Salt = salt,
            Iterations = _options.Iterations,
            MemorySize = _options.MemorySizeKb,
            DegreeOfParallelism = _options.DegreeOfParallelism,
        };

        return argon2.GetBytesAsync(32).GetAwaiter().GetResult();
    }

    private byte[] CombinePepper(byte[] passwordBytes)
    {
        byte[] combined = new byte[_pepperBytes.Length + passwordBytes.Length];
        Buffer.BlockCopy(_pepperBytes, 0, combined, 0, _pepperBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, _pepperBytes.Length, passwordBytes.Length);
        return combined;
    }

    private static void ValidateOptions(RefreshTokenOptions options)
    {
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
