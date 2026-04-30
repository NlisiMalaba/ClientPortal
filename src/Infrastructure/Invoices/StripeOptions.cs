namespace Infrastructure.Invoices;

public sealed class StripeOptions
{
    public const string SectionName = "Payments:Stripe";

    public string BaseUrl { get; set; } = "https://api.stripe.com";

    public string SecretKey { get; set; } = string.Empty;

    public string Currency { get; set; } = "USD";

    public int RequestTimeoutSeconds { get; set; } = 30;
}
