using Domain;
using Shared;

namespace Application.Invoices.Dtos;

public sealed record GetInvoicesResultDto(
    PagedResult<InvoiceListItemDto> Invoices,
    InvoiceAgingSummaryDto AgingSummary);

public sealed record InvoiceListItemDto(
    Guid Id,
    Guid ClientId,
    Guid ProjectId,
    string InvoiceNumber,
    InvoiceStatus Status,
    decimal Total,
    decimal AmountPaid,
    decimal OutstandingAmount,
    string Currency,
    DateOnly DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record InvoiceAgingSummaryDto(
    decimal Current,
    decimal Days1To30,
    decimal Days31To60,
    decimal Days61To90,
    decimal Days91Plus,
    decimal TotalOutstanding,
    int OverdueCount);
