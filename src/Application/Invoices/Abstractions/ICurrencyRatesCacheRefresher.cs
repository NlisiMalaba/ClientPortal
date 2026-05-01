namespace Application.Invoices.Abstractions;

public interface ICurrencyRatesCacheRefresher
{
    Task RefreshAsync(CancellationToken cancellationToken);
}
