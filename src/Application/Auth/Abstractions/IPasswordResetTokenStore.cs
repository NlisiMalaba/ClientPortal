namespace Application.Auth.Abstractions;

public interface IPasswordResetTokenStore
{
    Task StoreAsync(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);

    Task<PasswordResetTokenRecord?> FindValidByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task MarkUsedAsync(
        Guid userId,
        string tokenHash,
        DateTime usedAtUtc,
        CancellationToken cancellationToken = default);
}
