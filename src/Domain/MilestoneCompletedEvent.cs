namespace Domain;

public sealed record MilestoneCompletedEvent(Guid MilestoneId, Guid ProjectId, DateTime CompletedAt) : IDomainEvent;
