namespace Domain;

public sealed record ClientInvitedEvent(Guid ClientId, DateTime InvitedAt) : IDomainEvent;
