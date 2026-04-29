namespace Domain;

public sealed record ClientRequestSubmittedEvent(Guid RequestId, Guid ClientId, Guid ProjectId, DateTime SubmittedAt) : IDomainEvent;
