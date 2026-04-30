namespace Domain;

public sealed record MessageSentEvent(
    Guid MessageId,
    Guid ThreadId,
    Guid SenderId,
    DateTime SentAt) : IDomainEvent;
