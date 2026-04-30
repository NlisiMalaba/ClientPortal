namespace Domain;

public sealed record InvoiceSentEvent(
    Guid InvoiceId,
    Guid ClientId,
    DateTime SentAt) : IDomainEvent;
