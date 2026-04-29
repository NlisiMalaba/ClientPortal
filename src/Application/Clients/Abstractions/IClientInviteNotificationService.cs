using Domain;

namespace Application.Clients.Abstractions;

public interface IClientInviteNotificationService
{
    Task SendInviteAsync(
        Client client,
        User clientUser,
        string inviteToken,
        DateTime invitedAtUtc,
        CancellationToken cancellationToken = default);
}
