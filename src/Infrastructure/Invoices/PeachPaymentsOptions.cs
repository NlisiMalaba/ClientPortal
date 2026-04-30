namespace Infrastructure.Invoices;

public sealed class PeachPaymentsOptions
{
    public const string SectionName = "Payments:Peach";

    public string BaseUrl { get; set; } = "https://test.oppwa.com";

    public string EntityId { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string Currency { get; set; } = "ZAR";

    public int RequestTimeoutSeconds { get; set; } = 30;
}
