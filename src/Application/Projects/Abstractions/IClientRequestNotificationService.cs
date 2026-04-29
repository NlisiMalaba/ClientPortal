using Domain;

namespace Application.Projects.Abstractions;

public interface IClientRequestNotificationService
{
    Task NotifySubmittedAsync(ClientRequest request, CancellationToken cancellationToken = default);
}
