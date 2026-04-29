namespace Domain;

public sealed record ClientOnboardedEvent(Guid ClientId, DateTime OnboardedAt) : IDomainEvent;
