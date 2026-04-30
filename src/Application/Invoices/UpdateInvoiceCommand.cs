using MediatR;
using Shared;

namespace Application.Invoices;

public sealed record UpdateInvoiceCommand(
    Guid InvoiceId,
    Guid ClientId,
    string InvoiceNumber,
    string Currency,
    DateOnly DueDate,
    IReadOnlyCollection<CreateInvoiceLineItemInput> LineItems,
    string? Notes = null) : IRequest<Result>;
