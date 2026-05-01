using Application.Clients.Abstractions;
using Application.Documents.Abstractions;
using Application.Notifications.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Documents;

public sealed class ContractSignedEventHandler : INotificationHandler<ContractSignedEvent>
{
    private readonly IContractRepository _contractRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IContractBusinessStaffRecipientProvider _recipientProvider;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ContractSignedEventHandler> _logger;

    public ContractSignedEventHandler(
        IContractRepository contractRepository,
        IClientRepository clientRepository,
        IContractBusinessStaffRecipientProvider recipientProvider,
        INotificationService notificationService,
        ILogger<ContractSignedEventHandler> logger)
    {
        _contractRepository = contractRepository;
        _clientRepository = clientRepository;
        _recipientProvider = recipientProvider;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(ContractSignedEvent notification, CancellationToken cancellationToken)
    {
        Contract? contract = await _contractRepository.FindByIdAsync(notification.ContractId, cancellationToken);
        if (contract is null || contract.ClientId != notification.ClientId)
        {
            _logger.LogWarning(
                "ContractSignedEvent ignored because contract {ContractId} was not found for client {ClientId}.",
                notification.ContractId,
                notification.ClientId);
            return;
        }

        Client? client = await _clientRepository.FindByIdAsync(notification.ClientId, cancellationToken);
        if (client is null)
        {
            _logger.LogWarning(
                "ContractSignedEvent ignored because client {ClientId} was not found for contract {ContractId}.",
                notification.ClientId,
                notification.ContractId);
            return;
        }

        IReadOnlyList<string> recipients = _recipientProvider.GetSignedContractNotificationRecipients();
        if (recipients.Count == 0)
        {
            _logger.LogInformation(
                "No business staff recipients configured for signed-contract notifications. Contract {ContractId}.",
                contract.Id);
            return;
        }

        string subject = $"Contract signed: {contract.Title}";
        string body =
            $"Client {client.CompanyName} ({client.Email.Value}) signed contract \"{contract.Title}\".\n" +
            $"Signed at: {notification.SignedAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
            $"Contract ID: {contract.Id}.";

        foreach (string recipient in recipients)
        {
            try
            {
                await _notificationService.SendAsync(
                    new NotificationMessage(
                        NotificationChannel.Email,
                        recipient,
                        subject,
                        body),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send signed-contract notification for contract {ContractId} to {Recipient}.",
                    contract.Id,
                    recipient);
            }
        }
    }
}
