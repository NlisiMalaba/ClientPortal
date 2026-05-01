namespace Application.Notifications.Abstractions;

public interface IWeeklyDigestReader
{
    Task<IReadOnlyList<WeeklyDigestItem>> GetWeeklyDigestItemsAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken);
}
