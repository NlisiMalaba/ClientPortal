namespace Domain;

public sealed record InvoiceViewedEvent(
    Guid InvoiceId,
    Guid ClientId,
    DateTime ViewedAt) : IDomainEvent;
