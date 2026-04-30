using MediatR;
using Shared;

namespace Application.Invoices;

public sealed record RecordPaymentCommand(
    Guid InvoiceId,
    Guid ClientId,
    decimal Amount,
    string Currency,
    string Method,
    string Reference,
    DateTime PaidAtUtc,
    string? Notes = null) : IRequest<Result>;
