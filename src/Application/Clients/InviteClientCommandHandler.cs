using Application.Abstractions;
using Application.Auth.Abstractions;
using Application.Clients.Abstractions;
using Application.Clients.Dtos;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Clients;

public sealed class InviteClientCommandHandler : IRequestHandler<InviteClientCommand, Result<InviteClientResultDto>>
{
    private static readonly Error ClientEmailTakenError = new(
        "Clients.EmailTaken",
        "A client or user with this email already exists.",
        ErrorType.Conflict);

    private static readonly Error InviteEmailFailedError = new(
        "Clients.InviteEmailFailed",
        "Failed to send the client invite email.",
        ErrorType.Unexpected);

    private readonly IClientRepository _clientRepository;
    private readonly IClientUserAccountRepository _clientUserAccountRepository;
    private readonly IClientInvitationTokenService _clientInvitationTokenService;
    private readonly IClientInvitationTokenStore _clientInvitationTokenStore;
    private readonly IClientInviteNotificationService _clientInviteNotificationService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InviteClientCommandHandler> _logger;

    public InviteClientCommandHandler(
        IClientRepository clientRepository,
        IClientUserAccountRepository clientUserAccountRepository,
        IClientInvitationTokenService clientInvitationTokenService,
        IClientInvitationTokenStore clientInvitationTokenStore,
        IClientInviteNotificationService clientInviteNotificationService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<InviteClientCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _clientUserAccountRepository = clientUserAccountRepository;
        _clientInvitationTokenService = clientInvitationTokenService;
        _clientInvitationTokenStore = clientInvitationTokenStore;
        _clientInviteNotificationService = clientInviteNotificationService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<InviteClientResultDto>> Handle(InviteClientCommand request, CancellationToken cancellationToken)
    {
        EmailAddress email = new(request.Email);

        bool emailUsedByClient = await _clientRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailUsedByClient)
        {
            return Result<InviteClientResultDto>.Failure(ClientEmailTakenError);
        }

        bool emailUsedByUser = await _clientUserAccountRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailUsedByUser)
        {
            return Result<InviteClientResultDto>.Failure(ClientEmailTakenError);
        }

        Guid clientId = Guid.CreateVersion7();
        Client client = Client.Invite(
            id: clientId,
            companyName: request.CompanyName,
            contactName: request.ContactName,
            email: email,
            phone: new PhoneNumber(request.Phone),
            notes: request.Notes);

        Guid clientUserId = Guid.CreateVersion7();
        ClientInvitationTokenIssueResult inviteToken = _clientInvitationTokenService.Issue();
        string temporaryPassword = Guid.CreateVersion7().ToString("N");
        User clientUser = User.Create(
            id: clientUserId,
            email: email,
            fullName: request.ContactName,
            passwordHash: _passwordHasher.Hash(temporaryPassword),
            role: Role.ClientUser,
            isActive: false);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _clientRepository.Add(client);
            _clientUserAccountRepository.Add(clientUser);
            await _clientInvitationTokenStore.StoreAsync(
                client.Id,
                clientUser.Id,
                inviteToken.TokenHash,
                inviteToken.ExpiresAtUtc,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        try
        {
            await _clientInviteNotificationService.SendInviteAsync(
                client,
                clientUser,
                inviteToken.Token,
                client.InvitedAt,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send invite email for client {ClientId} and user {ClientUserId}.",
                client.Id,
                clientUser.Id);
            return Result<InviteClientResultDto>.Failure(InviteEmailFailedError);
        }

        InviteClientResultDto result = new(client.Id, clientUser.Id);
        return Result<InviteClientResultDto>.Success(result);
    }
}
