namespace Application.Meetings.Abstractions;

public sealed record MeetingReminderItem(
    string TenantSlug,
    Guid MeetingId,
    Guid ClientId,
    string MeetingTitle,
    DateTime ScheduledAtUtc,
    string MeetingUrl,
    string ClientContactName,
    string ClientEmail,
    string ClientPhone,
    MeetingReminderLeadTime LeadTime);
