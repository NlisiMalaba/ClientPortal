using Application.Clients.Dtos;
using Domain;
using Shared;

namespace Application.Clients.Abstractions;

public interface IClientRepository
{
    Task<Client?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);

    Task<Client?> GetByInviteTokenAsync(string inviteToken, CancellationToken cancellationToken = default);

    Task<Client?> GetWithProjectsAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);

    Task<Client?> FindByIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<ClientDetailDto?> GetDetailByIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<PagedResult<ClientListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        ClientStatus? status,
        CancellationToken cancellationToken = default);

    void Add(Client client);

    void Update(Client client);
}
