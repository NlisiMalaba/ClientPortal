using Application.Invoices.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Invoices;

public sealed class CurrencyRateRefreshJob
{
    private readonly ICurrencyRatesCacheRefresher _currencyRatesCacheRefresher;
    private readonly ILogger<CurrencyRateRefreshJob> _logger;

    public CurrencyRateRefreshJob(
        ICurrencyRatesCacheRefresher currencyRatesCacheRefresher,
        ILogger<CurrencyRateRefreshJob> logger)
    {
        _currencyRatesCacheRefresher = currencyRatesCacheRefresher;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await _currencyRatesCacheRefresher.RefreshAsync(cancellationToken);
        _logger.LogInformation("CurrencyRateRefreshJob refreshed cached exchange rates.");
    }
}
