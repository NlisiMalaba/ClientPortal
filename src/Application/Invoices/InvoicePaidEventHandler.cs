using Application.Clients.Abstractions;
using Application.Invoices.Abstractions;
using Application.Notifications.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Invoices;

public sealed class InvoicePaidEventHandler : INotificationHandler<InvoicePaidEvent>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IInvoiceBusinessStaffRecipientProvider _businessStaffRecipientProvider;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InvoicePaidEventHandler> _logger;

    public InvoicePaidEventHandler(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IInvoiceBusinessStaffRecipientProvider businessStaffRecipientProvider,
        INotificationService notificationService,
        ILogger<InvoicePaidEventHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _businessStaffRecipientProvider = businessStaffRecipientProvider;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(InvoicePaidEvent notification, CancellationToken cancellationToken)
    {
        Invoice? invoice = await _invoiceRepository.FindByIdAsync(notification.InvoiceId, cancellationToken);
        if (invoice is null || invoice.ClientId != notification.ClientId)
        {
            _logger.LogWarning(
                "InvoicePaidEvent ignored because invoice {InvoiceId} was not found for client {ClientId}.",
                notification.InvoiceId,
                notification.ClientId);
            return;
        }

        Client? client = await _clientRepository.FindByIdAsync(notification.ClientId, cancellationToken);
        if (client is null)
        {
            _logger.LogWarning(
                "InvoicePaidEvent ignored because client {ClientId} was not found for invoice {InvoiceId}.",
                notification.ClientId,
                notification.InvoiceId);
            return;
        }

        await SendClientReceiptEmailAsync(client, invoice, notification.PaidAt, cancellationToken);
        await SendBusinessStaffConfirmationAsync(client, invoice, notification.PaidAt, cancellationToken);
    }

    private async Task SendClientReceiptEmailAsync(
        Client client,
        Invoice invoice,
        DateTime paidAtUtc,
        CancellationToken cancellationToken)
    {
        string subject = $"Payment received for invoice {invoice.InvoiceNumber}";
        string body =
            $"Hello {client.ContactName},\n\n" +
            $"We received your payment for invoice {invoice.InvoiceNumber}.\n" +
            $"Amount received: {invoice.Total:0.00} {invoice.Currency}\n" +
            $"Paid at: {paidAtUtc:yyyy-MM-dd HH:mm:ss} UTC\n\n" +
            "Thank you.";

        try
        {
            await _notificationService.SendAsync(
                new NotificationMessage(
                    NotificationChannel.Email,
                    client.Email.Value,
                    subject,
                    body),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send invoice receipt email for invoice {InvoiceId}.",
                invoice.Id);
        }
    }

    private async Task SendBusinessStaffConfirmationAsync(
        Client client,
        Invoice invoice,
        DateTime paidAtUtc,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> recipients = _businessStaffRecipientProvider.GetReceiptConfirmationEmailRecipients();
        if (recipients.Count == 0)
        {
            _logger.LogInformation(
                "No business staff recipients configured for paid-invoice confirmations. Invoice {InvoiceId}.",
                invoice.Id);
            return;
        }

        string subject = $"Invoice paid: {invoice.InvoiceNumber}";
        string body =
            $"Client {client.CompanyName} ({client.Email.Value}) paid invoice {invoice.InvoiceNumber}.\n" +
            $"Amount: {invoice.Total:0.00} {invoice.Currency}\n" +
            $"Paid at: {paidAtUtc:yyyy-MM-dd HH:mm:ss} UTC.";

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
                    "Failed to send business staff paid-invoice confirmation for invoice {InvoiceId} to {Recipient}.",
                    invoice.Id,
                    recipient);
            }
        }
    }
}
