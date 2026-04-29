using Application.Clients.Abstractions;
using Application.Clients.Dtos;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDetailDto>>
{
    private static readonly Error ClientNotFoundError = new(
        "Clients.NotFound",
        "Client was not found.",
        ErrorType.NotFound);

    private readonly IClientRepository _clientRepository;

    public GetClientByIdQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Result<ClientDetailDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        ClientDetailDto? client = await _clientRepository.GetDetailByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            return Result<ClientDetailDto>.Failure(ClientNotFoundError);
        }

        return Result<ClientDetailDto>.Success(client);
    }
}
