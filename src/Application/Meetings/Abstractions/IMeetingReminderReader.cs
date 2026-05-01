namespace Application.Meetings.Abstractions;

public interface IMeetingReminderReader
{
    Task<IReadOnlyList<MeetingReminderItem>> GetPendingRemindersAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken);
}
