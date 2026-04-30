using Application.Invoices.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed record GetInvoicesQuery(
    int Page = 1,
    int PageSize = 20,
    InvoiceStatus? Status = null,
    Guid? ClientId = null,
    DateOnly? DueDateFrom = null,
    DateOnly? DueDateTo = null,
    DateOnly? AsOfDate = null) : IRequest<Result<GetInvoicesResultDto>>;
