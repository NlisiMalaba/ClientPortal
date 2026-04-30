namespace Application.Invoices.Abstractions;

public sealed record CurrencyConversionResult(
    decimal SourceAmount,
    string SourceCurrency,
    decimal ConvertedAmount,
    string TargetCurrency,
    decimal Rate,
    DateTime RateAsOfUtc);
