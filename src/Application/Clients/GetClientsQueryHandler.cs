using Application.Clients.Abstractions;
using Application.Clients.Dtos;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, Result<PagedResult<ClientListItemDto>>>
{
    private readonly IClientRepository _clientRepository;

    public GetClientsQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Result<PagedResult<ClientListItemDto>>> Handle(
        GetClientsQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<ClientListItemDto> clients = await _clientRepository.GetPagedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            search: NormalizeSearch(request.Search),
            status: request.Status,
            cancellationToken: cancellationToken);

        return Result<PagedResult<ClientListItemDto>>.Success(clients);
    }

    private static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        return search.Trim();
    }
}
