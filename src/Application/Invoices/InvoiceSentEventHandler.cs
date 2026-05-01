using Application.Clients.Abstractions;
using Application.Invoices.Abstractions;
using Application.Notifications.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Invoices;

public sealed class InvoiceSentEventHandler : INotificationHandler<InvoiceSentEvent>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IInvoicePdfGenerator _invoicePdfGenerator;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InvoiceSentEventHandler> _logger;

    public InvoiceSentEventHandler(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IInvoicePdfGenerator invoicePdfGenerator,
        INotificationService notificationService,
        ILogger<InvoiceSentEventHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _invoicePdfGenerator = invoicePdfGenerator;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(InvoiceSentEvent notification, CancellationToken cancellationToken)
    {
        Invoice? invoice = await _invoiceRepository.FindByIdAsync(notification.InvoiceId, cancellationToken);
        if (invoice is null || invoice.ClientId != notification.ClientId)
        {
            _logger.LogWarning(
                "InvoiceSentEvent ignored because invoice {InvoiceId} was not found for client {ClientId}.",
                notification.InvoiceId,
                notification.ClientId);
            return;
        }

        Client? client = await _clientRepository.FindByIdAsync(notification.ClientId, cancellationToken);
        if (client is null)
        {
            _logger.LogWarning(
                "InvoiceSentEvent ignored because client {ClientId} was not found for invoice {InvoiceId}.",
                notification.ClientId,
                notification.InvoiceId);
            return;
        }

        InvoicePdfDocument invoicePdf;
        try
        {
            invoicePdf = await _invoicePdfGenerator.GenerateAsync(invoice, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for sent invoice {InvoiceId}.", invoice.Id);
            return;
        }

        await SendEmailWithAttachmentAsync(client, invoice, invoicePdf, cancellationToken);
        await SendWhatsAppNotificationAsync(client, invoice, cancellationToken);
    }

    private async Task SendEmailWithAttachmentAsync(
        Client client,
        Invoice invoice,
        InvoicePdfDocument invoicePdf,
        CancellationToken cancellationToken)
    {
        string subject = $"Invoice {invoice.InvoiceNumber} is ready";
        string body =
            $"Hello {client.ContactName},\n\n" +
            $"Your invoice {invoice.InvoiceNumber} has been sent.\n" +
            $"Total: {invoice.Total:0.00} {invoice.Currency}\n" +
            $"Due date: {invoice.DueDate:yyyy-MM-dd}\n\n" +
            "Your PDF invoice is attached to this email.";

        Dictionary<string, string> metadata = new(StringComparer.Ordinal)
        {
            ["attachment.fileName"] = invoicePdf.FileName,
            ["attachment.contentType"] = "application/pdf",
            ["attachment.base64"] = Convert.ToBase64String(invoicePdf.Content)
        };

        try
        {
            await _notificationService.SendAsync(
                new NotificationMessage(
                    NotificationChannel.Email,
                    client.Email.Value,
                    subject,
                    body,
                    metadata),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send invoice email notification for invoice {InvoiceId}.",
                invoice.Id);
        }
    }

    private async Task SendWhatsAppNotificationAsync(
        Client client,
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        string subject = $"Invoice {invoice.InvoiceNumber}";
        string body =
            $"Your invoice is ready. Total {invoice.Total:0.00} {invoice.Currency}. " +
            $"Due {invoice.DueDate:yyyy-MM-dd}.";

        try
        {
            await _notificationService.SendAsync(
                new NotificationMessage(
                    NotificationChannel.WhatsApp,
                    client.Phone.Value,
                    subject,
                    body),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send invoice WhatsApp notification for invoice {InvoiceId}.",
                invoice.Id);
        }
    }
}
