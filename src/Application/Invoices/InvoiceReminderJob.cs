using Application.Invoices.Abstractions;
using Application.Notifications.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Invoices;

public sealed class InvoiceReminderJob
{
    private readonly IOverdueInvoiceReminderReader _overdueInvoiceReminderReader;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InvoiceReminderJob> _logger;

    public InvoiceReminderJob(
        IOverdueInvoiceReminderReader overdueInvoiceReminderReader,
        INotificationService notificationService,
        ILogger<InvoiceReminderJob> logger)
    {
        _overdueInvoiceReminderReader = overdueInvoiceReminderReader;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DateOnly asOfDate = DateOnly.FromDateTime(DateTime.UtcNow);
        IReadOnlyList<OverdueInvoiceReminderItem> overdueInvoices = await _overdueInvoiceReminderReader
            .GetOverdueInvoicesAsync(asOfDate, cancellationToken);

        foreach (OverdueInvoiceReminderItem item in overdueInvoices)
        {
            await SendEmailReminderAsync(item, cancellationToken);
            await SendWhatsAppReminderAsync(item, cancellationToken);
        }

        _logger.LogInformation(
            "InvoiceReminderJob processed {Count} overdue invoices for reminders as of {AsOfDate}.",
            overdueInvoices.Count,
            asOfDate);
    }

    private async Task SendEmailReminderAsync(OverdueInvoiceReminderItem item, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(item.ClientEmail))
        {
            _logger.LogWarning(
                "Skipping email reminder for invoice {InvoiceId} in tenant {TenantSlug} because client email is missing.",
                item.InvoiceId,
                item.TenantSlug);
            return;
        }

        string subject = $"Payment reminder: Invoice {item.InvoiceNumber} is overdue";
        string body =
            $"Hello {item.ClientContactName},\n\n" +
            $"This is a reminder that invoice {item.InvoiceNumber} is overdue.\n" +
            $"Outstanding amount: {item.OutstandingAmount:0.00} {item.Currency}\n" +
            $"Due date: {item.DueDate:yyyy-MM-dd}\n\n" +
            "Please make payment at your earliest convenience.";

        try
        {
            await _notificationService.SendAsync(
                new NotificationMessage(
                    NotificationChannel.Email,
                    item.ClientEmail,
                    subject,
                    body),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email reminder for invoice {InvoiceId} in tenant {TenantSlug}.",
                item.InvoiceId,
                item.TenantSlug);
        }
    }

    private async Task SendWhatsAppReminderAsync(OverdueInvoiceReminderItem item, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(item.ClientPhone))
        {
            _logger.LogWarning(
                "Skipping WhatsApp reminder for invoice {InvoiceId} in tenant {TenantSlug} because client phone is missing.",
                item.InvoiceId,
                item.TenantSlug);
            return;
        }

        string subject = $"Invoice {item.InvoiceNumber} overdue";
        string body =
            $"Reminder: invoice {item.InvoiceNumber} is overdue. " +
            $"Outstanding {item.OutstandingAmount:0.00} {item.Currency}. " +
            $"Due {item.DueDate:yyyy-MM-dd}.";

        try
        {
            await _notificationService.SendAsync(
                new NotificationMessage(
                    NotificationChannel.WhatsApp,
                    item.ClientPhone,
                    subject,
                    body),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send WhatsApp reminder for invoice {InvoiceId} in tenant {TenantSlug}.",
                item.InvoiceId,
                item.TenantSlug);
        }
    }
}
