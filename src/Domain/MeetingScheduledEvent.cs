namespace Domain;

public sealed record MeetingScheduledEvent(
    Guid MeetingId,
    Guid ClientId,
    DateTime ScheduledAt,
    DateTime OccursAt) : IDomainEvent;
