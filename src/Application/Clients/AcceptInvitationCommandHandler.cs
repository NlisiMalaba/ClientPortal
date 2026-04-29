using Application.Abstractions;
using Application.Auth.Abstractions;
using Application.Clients.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Clients;

public sealed class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result>
{
    private static readonly Error InvalidInviteTokenError = new(
        "Clients.InvalidInviteToken",
        "Invitation token is invalid or expired.",
        ErrorType.Forbidden);

    private readonly IClientRepository _clientRepository;
    private readonly IClientUserAccountRepository _clientUserAccountRepository;
    private readonly IClientInvitationTokenService _clientInvitationTokenService;
    private readonly IClientInvitationTokenStore _clientInvitationTokenStore;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptInvitationCommandHandler(
        IClientRepository clientRepository,
        IClientUserAccountRepository clientUserAccountRepository,
        IClientInvitationTokenService clientInvitationTokenService,
        IClientInvitationTokenStore clientInvitationTokenStore,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _clientUserAccountRepository = clientUserAccountRepository;
        _clientInvitationTokenService = clientInvitationTokenService;
        _clientInvitationTokenStore = clientInvitationTokenStore;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = _clientInvitationTokenService.Hash(request.Token);
        ClientInvitationTokenRecord? tokenRecord = await _clientInvitationTokenStore.FindValidByHashAsync(tokenHash, cancellationToken);
        if (tokenRecord is null || tokenRecord.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return Result.Failure(InvalidInviteTokenError);
        }

        Client? client = await _clientRepository.FindByIdAsync(tokenRecord.ClientId, cancellationToken);
        User? clientUser = await _clientUserAccountRepository.FindByIdAsync(tokenRecord.UserId, cancellationToken);
        if (client is null || clientUser is null)
        {
            return Result.Failure(InvalidInviteTokenError);
        }

        clientUser.UpdatePasswordHash(_passwordHasher.Hash(request.Password));
        clientUser.Activate();
        client.Onboard(DateTime.UtcNow);

        _clientUserAccountRepository.Update(clientUser);
        _clientRepository.Update(client);

        await _clientInvitationTokenStore.MarkUsedAsync(
            client.Id,
            clientUser.Id,
            tokenRecord.TokenHash,
            DateTime.UtcNow,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
