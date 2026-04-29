namespace Domain;

public sealed record ClientDeactivatedEvent(Guid ClientId, DateTime DeactivatedAt) : IDomainEvent;
