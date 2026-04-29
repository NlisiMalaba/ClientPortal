using Application.Abstractions;
using Application.Auth.Abstractions;
using Application.Clients.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed class DeactivateClientCommandHandler : IRequestHandler<DeactivateClientCommand, Result>
{
    private static readonly Error ClientNotFoundError = new(
        "Clients.NotFound",
        "Client was not found.",
        ErrorType.NotFound);

    private readonly IClientRepository _clientRepository;
    private readonly IUserAuthenticationRepository _userAuthenticationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateClientCommandHandler(
        IClientRepository clientRepository,
        IUserAuthenticationRepository userAuthenticationRepository,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _userAuthenticationRepository = userAuthenticationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
    {
        Client? client = await _clientRepository.FindByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            return Result.Failure(ClientNotFoundError);
        }

        client.Deactivate(DateTime.UtcNow);
        _clientRepository.Update(client);

        // Current model links portal access via the client contact email.
        User? clientUser = await _userAuthenticationRepository.FindByEmailAsync(client.Email, cancellationToken);
        if (clientUser is not null)
        {
            clientUser.Deactivate();
            clientUser.RevokeRefreshTokenFamily(DateTime.UtcNow);
            _userAuthenticationRepository.Update(clientUser);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
