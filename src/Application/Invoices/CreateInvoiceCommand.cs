using Application.Invoices.Dtos;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed record CreateInvoiceCommand(
    Guid ClientId,
    Guid ProjectId,
    string InvoiceNumber,
    string Currency,
    DateOnly DueDate,
    IReadOnlyCollection<CreateInvoiceLineItemInput> LineItems,
    string? Notes = null) : IRequest<Result<InvoiceDto>>;

public sealed record CreateInvoiceLineItemInput(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate);
