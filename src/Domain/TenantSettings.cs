using Shared;

namespace Domain;

public sealed class TenantSettings : ValueObject
{
    public string BrandColour { get; }

    public string? LogoUrl { get; }

    public string DefaultCurrency { get; }

    public IReadOnlyCollection<string> NotificationChannels { get; }

    public string TaxConfig { get; }

    public TenantSettings(
        string brandColour,
        string? logoUrl,
        string defaultCurrency,
        IEnumerable<string>? notificationChannels,
        string taxConfig)
    {
        BrandColour = Guard.NotEmpty(brandColour, nameof(brandColour)).Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        DefaultCurrency = Guard.NotEmpty(defaultCurrency, nameof(defaultCurrency)).Trim().ToUpperInvariant();
        TaxConfig = Guard.NotEmpty(taxConfig, nameof(taxConfig)).Trim();

        List<string> normalizedChannels = (notificationChannels ?? [])
            .Where(channel => !string.IsNullOrWhiteSpace(channel))
            .Select(channel => channel.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        NotificationChannels = normalizedChannels.AsReadOnly();
    }

    public static TenantSettings Default()
    {
        return new TenantSettings(
            brandColour: "#2563EB",
            logoUrl: null,
            defaultCurrency: "USD",
            notificationChannels: ["email"],
            taxConfig: "{}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return BrandColour;
        yield return LogoUrl;
        yield return DefaultCurrency;

        foreach (string channel in NotificationChannels)
        {
            yield return channel;
        }

        yield return TaxConfig;
    }
}
