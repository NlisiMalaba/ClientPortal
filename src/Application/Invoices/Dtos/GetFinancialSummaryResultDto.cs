namespace Application.Invoices.Dtos;

public sealed record GetFinancialSummaryResultDto(
    decimal TotalOutstanding,
    decimal PaidThisMonth,
    int OverdueCount,
    IReadOnlyCollection<CashflowPointDto> Cashflow);

public sealed record CashflowPointDto(
    DateOnly Period,
    decimal Inflow,
    decimal Outflow,
    decimal NetCashflow);
