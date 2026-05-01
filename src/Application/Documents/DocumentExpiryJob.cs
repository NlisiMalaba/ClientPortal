using Application.Documents.Abstractions;
using Application.Notifications.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Documents;

public sealed class DocumentExpiryJob
{
    private readonly IExpiringContractAlertReader _expiringContractAlertReader;
    private readonly IContractBusinessStaffRecipientProvider _recipientProvider;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DocumentExpiryJob> _logger;

    public DocumentExpiryJob(
        IExpiringContractAlertReader expiringContractAlertReader,
        IContractBusinessStaffRecipientProvider recipientProvider,
        INotificationService notificationService,
        ILogger<DocumentExpiryJob> logger)
    {
        _expiringContractAlertReader = expiringContractAlertReader;
        _recipientProvider = recipientProvider;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DateOnly asOfDate = DateOnly.FromDateTime(DateTime.UtcNow);
        IReadOnlyList<ExpiringContractAlertItem> contracts = await _expiringContractAlertReader
            .GetExpiringContractsAsync(asOfDate, cancellationToken);
        IReadOnlyList<string> recipients = _recipientProvider.GetExpiringContractNotificationRecipients();

        if (contracts.Count == 0)
        {
            _logger.LogInformation("DocumentExpiryJob found no contracts expiring in 30/7/1-day windows.");
            return;
        }

        if (recipients.Count == 0)
        {
            _logger.LogInformation("DocumentExpiryJob skipped notifications because no recipients are configured.");
            return;
        }

        foreach (ExpiringContractAlertItem contract in contracts)
        {
            foreach (string recipient in recipients)
            {
                await SendAlertAsync(recipient, contract, cancellationToken);
            }
        }

        _logger.LogInformation(
            "DocumentExpiryJob sent alerts for {ContractCount} contracts to {RecipientCount} recipients.",
            contracts.Count,
            recipients.Count);
    }

    private async Task SendAlertAsync(
        string recipient,
        ExpiringContractAlertItem contract,
        CancellationToken cancellationToken)
    {
        string subject = $"Contract expiry alert ({contract.DaysUntilExpiry} day(s)): {contract.ContractTitle}";
        string body =
            $"Contract \"{contract.ContractTitle}\" is due to expire in {contract.DaysUntilExpiry} day(s).\n" +
            $"Expiry: {contract.ExpiresAtUtc:yyyy-MM-dd HH:mm:ss} UTC\n" +
            $"Client: {contract.ClientCompanyName} ({contract.ClientEmail})\n" +
            $"Tenant: {contract.TenantSlug}\n" +
            $"Contract ID: {contract.ContractId}";

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
                "Failed to send contract expiry alert for contract {ContractId} to {Recipient}.",
                contract.ContractId,
                recipient);
        }
    }
}
