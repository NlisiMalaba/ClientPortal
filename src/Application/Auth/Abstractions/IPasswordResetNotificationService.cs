using Domain;

namespace Application.Auth.Abstractions;

public interface IPasswordResetNotificationService
{
    Task SendResetPasswordAsync(
        User user,
        string resetToken,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);
}
