namespace Application.Notifications.Abstractions;

public sealed record WeeklyDigestItem(
    string TenantSlug,
    string RecipientEmail,
    int ActiveClients,
    int OpenInvoices,
    int OverdueInvoices,
    decimal OverdueAmountTotal,
    int UpcomingMeetingsNext7Days,
    int ContractsExpiringNext30Days);
