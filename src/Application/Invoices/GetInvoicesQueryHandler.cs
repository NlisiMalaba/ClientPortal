using Application.Invoices.Abstractions;
using Application.Invoices.Dtos;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<GetInvoicesResultDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<GetInvoicesResultDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        PagedResult<InvoiceListItemDto> invoices = await _invoiceRepository.GetPagedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            status: request.Status,
            clientId: request.ClientId,
            dueDateFrom: request.DueDateFrom,
            dueDateTo: request.DueDateTo,
            cancellationToken: cancellationToken);

        InvoiceAgingSummaryDto agingSummary = await _invoiceRepository.GetAgingSummaryAsync(
            status: request.Status,
            clientId: request.ClientId,
            dueDateFrom: request.DueDateFrom,
            dueDateTo: request.DueDateTo,
            asOfDate: request.AsOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            cancellationToken: cancellationToken);

        GetInvoicesResultDto result = new(invoices, agingSummary);
        return Result<GetInvoicesResultDto>.Success(result);
    }
}
