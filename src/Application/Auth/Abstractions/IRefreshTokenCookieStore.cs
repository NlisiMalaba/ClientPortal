namespace Application.Auth.Abstractions;

public interface IRefreshTokenCookieStore
{
    Task SetAsync(string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
