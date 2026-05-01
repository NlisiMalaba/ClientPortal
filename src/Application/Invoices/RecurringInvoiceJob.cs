using Application.Invoices.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Invoices;

public sealed class RecurringInvoiceJob
{
    private readonly IRecurringInvoiceGenerator _recurringInvoiceGenerator;
    private readonly ILogger<RecurringInvoiceJob> _logger;

    public RecurringInvoiceJob(
        IRecurringInvoiceGenerator recurringInvoiceGenerator,
        ILogger<RecurringInvoiceJob> logger)
    {
        _recurringInvoiceGenerator = recurringInvoiceGenerator;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DateOnly runDate = DateOnly.FromDateTime(DateTime.UtcNow);
        int generatedCount = await _recurringInvoiceGenerator.GenerateDailyAsync(runDate, cancellationToken);

        _logger.LogInformation(
            "RecurringInvoiceJob generated {GeneratedCount} recurring invoices for date {RunDate}.",
            generatedCount,
            runDate);
    }
}
