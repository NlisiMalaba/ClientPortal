using MediatR;
using Shared;

namespace Application.Invoices;

public sealed record SendInvoiceCommand(
    Guid InvoiceId,
    Guid ClientId) : IRequest<Result>;
