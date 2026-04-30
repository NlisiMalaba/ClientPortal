namespace Application.Invoices.Abstractions;

public interface ICurrencyConverter
{
    IReadOnlyCollection<string> SupportedCurrencies { get; }

    Task<CurrencyConversionResult> ConvertAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken);
}
