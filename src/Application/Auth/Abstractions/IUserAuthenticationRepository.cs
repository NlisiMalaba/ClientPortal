using Domain;

namespace Application.Auth.Abstractions;

public interface IUserAuthenticationRepository
{
    Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);

    Task<User?> FindByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Update(User user);
}
