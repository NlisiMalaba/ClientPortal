using Application.Invoices.Dtos;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed record GetFinancialSummaryQuery(
    Guid? ClientId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    DateOnly? AsOfDate = null) : IRequest<Result<GetFinancialSummaryResultDto>>;
