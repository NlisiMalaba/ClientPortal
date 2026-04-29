namespace Domain;

public sealed record TaskStatusChangedEvent(
    Guid TaskId,
    Guid ProjectId,
    ProjectTaskStatus PreviousStatus,
    ProjectTaskStatus CurrentStatus,
    DateTime ChangedAt) : IDomainEvent;
