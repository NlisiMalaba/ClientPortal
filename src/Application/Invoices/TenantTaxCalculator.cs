using System.Text.Json;
using Application.Invoices.Abstractions;
using Domain;

namespace Application.Invoices;

public sealed class TenantTaxCalculator : ITaxCalculator
{
    private const decimal ZaVatRate = 0.15m;

    public decimal ResolveDefaultRate(TenantSettings? tenantSettings)
    {
        if (tenantSettings is null || string.IsNullOrWhiteSpace(tenantSettings.TaxConfig))
        {
            return 0m;
        }

        TaxConfigModel? taxConfig = ParseTaxConfig(tenantSettings.TaxConfig);
        if (taxConfig is null)
        {
            return 0m;
        }

        string countryCode = NormalizeCountryCode(taxConfig.CountryCode);
        decimal? configuredCountryRate = ResolveConfiguredCountryRate(countryCode, taxConfig);
        decimal? configuredDefaultRate = NormalizeRate(taxConfig.DefaultRate ?? taxConfig.VatRate);

        return configuredCountryRate
            ?? configuredDefaultRate
            ?? ResolveRegionalDefaultRate(countryCode);
    }

    private static decimal? ResolveConfiguredCountryRate(string countryCode, TaxConfigModel config)
    {
        return countryCode switch
        {
            "ZA" => NormalizeRate(config.ZaVatRate ?? config.VatRate),
            "ZW" => NormalizeRate(config.ZwZimraRate),
            "ZM" => NormalizeRate(config.ZmZraRate),
            _ => null,
        };
    }

    private static decimal ResolveRegionalDefaultRate(string countryCode)
    {
        return countryCode switch
        {
            "ZA" => ZaVatRate,
            "ZW" => 0m,
            "ZM" => 0m,
            _ => 0m,
        };
    }

    private static string NormalizeCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return string.Empty;
        }

        return countryCode.Trim().ToUpperInvariant();
    }

    private static decimal? NormalizeRate(decimal? rate)
    {
        if (!rate.HasValue)
        {
            return null;
        }

        if (rate < 0m || rate > 1m)
        {
            return null;
        }

        return decimal.Round(rate.Value, 4, MidpointRounding.ToEven);
    }

    private static TaxConfigModel? ParseTaxConfig(string taxConfigJson)
    {
        try
        {
            return JsonSerializer.Deserialize<TaxConfigModel>(
                taxConfigJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record TaxConfigModel(
        string? CountryCode,
        decimal? DefaultRate,
        decimal? VatRate,
        decimal? ZaVatRate,
        decimal? ZwZimraRate,
        decimal? ZmZraRate);
}
