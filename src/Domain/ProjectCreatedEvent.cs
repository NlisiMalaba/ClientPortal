namespace Domain;

public sealed record ProjectCreatedEvent(Guid ProjectId, Guid ClientId, DateTime CreatedAt) : IDomainEvent;
