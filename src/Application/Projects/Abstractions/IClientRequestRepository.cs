using Domain;

namespace Application.Projects.Abstractions;

public interface IClientRequestRepository
{
    Task<ClientRequest?> FindByIdAsync(Guid requestId, CancellationToken cancellationToken = default);

    void Add(ClientRequest request);

    void Update(ClientRequest request);
}
