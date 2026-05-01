namespace Application.Invoices.Abstractions;

public interface IOverdueInvoiceReminderReader
{
    Task<IReadOnlyList<OverdueInvoiceReminderItem>> GetOverdueInvoicesAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken);
}
