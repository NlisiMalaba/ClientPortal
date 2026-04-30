using Application.Invoices.Abstractions;
using Application.Invoices.Dtos;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed class GetFinancialSummaryQueryHandler : IRequestHandler<GetFinancialSummaryQuery, Result<GetFinancialSummaryResultDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetFinancialSummaryQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<GetFinancialSummaryResultDto>> Handle(
        GetFinancialSummaryQuery request,
        CancellationToken cancellationToken)
    {
        GetFinancialSummaryResultDto summary = await _invoiceRepository.GetFinancialSummaryAsync(
            clientId: request.ClientId,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            asOfDate: request.AsOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            cancellationToken: cancellationToken);

        return Result<GetFinancialSummaryResultDto>.Success(summary);
    }
}
