namespace Domain;

public sealed record InvoicePaidEvent(
    Guid InvoiceId,
    Guid ClientId,
    DateTime PaidAt) : IDomainEvent;
