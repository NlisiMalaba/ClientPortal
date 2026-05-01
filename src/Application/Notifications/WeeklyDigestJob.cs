using Application.Notifications.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed class WeeklyDigestJob
{
    private readonly IWeeklyDigestReader _weeklyDigestReader;
    private readonly INotificationService _notificationService;
    private readonly ILogger<WeeklyDigestJob> _logger;

    public WeeklyDigestJob(
        IWeeklyDigestReader weeklyDigestReader,
        INotificationService notificationService,
        ILogger<WeeklyDigestJob> logger)
    {
        _weeklyDigestReader = weeklyDigestReader;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;
        IReadOnlyList<WeeklyDigestItem> digestItems = await _weeklyDigestReader
            .GetWeeklyDigestItemsAsync(nowUtc, cancellationToken);

        foreach (WeeklyDigestItem item in digestItems)
        {
            await SendDigestAsync(item, nowUtc, cancellationToken);
        }

        _logger.LogInformation(
            "WeeklyDigestJob sent {DigestCount} owner digest messages at {NowUtc}.",
            digestItems.Count,
            nowUtc);
    }

    private async Task SendDigestAsync(
        WeeklyDigestItem digest,
        DateTime generatedAtUtc,
        CancellationToken cancellationToken)
    {
        string subject = $"Weekly business digest ({digest.TenantSlug})";
        string body =
            "Your weekly business summary is ready.\n\n" +
            $"Generated at: {generatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC\n" +
            $"Tenant: {digest.TenantSlug}\n" +
            $"Active clients: {digest.ActiveClients}\n" +
            $"Open invoices: {digest.OpenInvoices}\n" +
            $"Overdue invoices: {digest.OverdueInvoices}\n" +
            $"Overdue amount total: {digest.OverdueAmountTotal:0.00}\n" +
            $"Upcoming meetings (next 7 days): {digest.UpcomingMeetingsNext7Days}\n" +
            $"Contracts expiring (next 30 days): {digest.ContractsExpiringNext30Days}";

        try
        {
            await _notificationService.SendAsync(
                new NotificationMessage(
                    NotificationChannel.Email,
                    digest.RecipientEmail,
                    subject,
                    body),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send weekly digest for tenant {TenantSlug} to {Recipient}.",
                digest.TenantSlug,
                digest.RecipientEmail);
        }
    }
}
