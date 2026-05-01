namespace Application.Invoices.Abstractions;

public interface IRecurringInvoiceGenerator
{
    Task<int> GenerateDailyAsync(DateOnly runDate, CancellationToken cancellationToken);
}
