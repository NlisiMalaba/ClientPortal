using System.Globalization;
using Application.Invoices.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Invoices;

public sealed class CachedCurrencyConverter : ICurrencyConverter
{
    private static readonly IReadOnlyDictionary<string, decimal> DefaultUsdRates = new Dictionary<string, decimal>(StringComparer.Ordinal)
    {
        ["USD"] = 1.00m,
        ["ZAR"] = 18.40m,
        ["ZWL"] = 322.00m,
        ["ZMW"] = 26.10m,
        ["MWK"] = 1735.00m,
        ["BWP"] = 13.75m,
        ["MZN"] = 63.80m,
        ["MUR"] = 46.20m,
    };

    private readonly IConfiguration _configuration;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);
    private readonly Lock _sync = new();

    private IReadOnlyDictionary<string, decimal> _cachedUsdRates = DefaultUsdRates;
    private DateTime _cacheExpiresAtUtc = DateTime.MinValue;

    public CachedCurrencyConverter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IReadOnlyCollection<string> SupportedCurrencies => _cachedUsdRates.Keys.ToList().AsReadOnly();

    public Task<CurrencyConversionResult> ConvertAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }

        string normalizedFrom = NormalizeCurrency(fromCurrency, nameof(fromCurrency));
        string normalizedTo = NormalizeCurrency(toCurrency, nameof(toCurrency));
        IReadOnlyDictionary<string, decimal> rates = GetRates();

        if (!rates.TryGetValue(normalizedFrom, out decimal fromRate))
        {
            throw new ArgumentException($"Unsupported source currency '{normalizedFrom}'.", nameof(fromCurrency));
        }

        if (!rates.TryGetValue(normalizedTo, out decimal toRate))
        {
            throw new ArgumentException($"Unsupported target currency '{normalizedTo}'.", nameof(toCurrency));
        }

        decimal conversionRate = decimal.Round(toRate / fromRate, 8, MidpointRounding.ToEven);
        decimal converted = decimal.Round(amount * conversionRate, 2, MidpointRounding.ToEven);

        CurrencyConversionResult result = new(
            SourceAmount: decimal.Round(amount, 2, MidpointRounding.ToEven),
            SourceCurrency: normalizedFrom,
            ConvertedAmount: converted,
            TargetCurrency: normalizedTo,
            Rate: conversionRate,
            RateAsOfUtc: DateTime.UtcNow);

        return Task.FromResult(result);
    }

    private IReadOnlyDictionary<string, decimal> GetRates()
    {
        DateTime now = DateTime.UtcNow;
        lock (_sync)
        {
            if (now < _cacheExpiresAtUtc)
            {
                return _cachedUsdRates;
            }

            _cachedUsdRates = LoadRatesFromConfiguration();
            _cacheExpiresAtUtc = now.Add(_cacheDuration);
            return _cachedUsdRates;
        }
    }

    private IReadOnlyDictionary<string, decimal> LoadRatesFromConfiguration()
    {
        IConfigurationSection section = _configuration.GetSection("CurrencyRates:UsdBase");
        Dictionary<string, decimal> rates = new(DefaultUsdRates, StringComparer.Ordinal);

        foreach (IConfigurationSection child in section.GetChildren())
        {
            string code = NormalizeCurrency(child.Key, child.Path);
            if (!decimal.TryParse(child.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedRate))
            {
                continue;
            }

            if (parsedRate <= 0m)
            {
                continue;
            }

            rates[code] = parsedRate;
        }

        return rates;
    }

    private static string NormalizeCurrency(string currency, string paramName)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", paramName);
        }

        string normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(ch => !char.IsAsciiLetter(ch)))
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", paramName);
        }

        return normalized;
    }
}
