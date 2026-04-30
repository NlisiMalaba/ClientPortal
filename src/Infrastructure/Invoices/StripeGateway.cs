using System.Net.Http.Headers;
using System.Text.Json;
using Application.Invoices.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Invoices;

public sealed class StripeGateway : IPaymentGateway
{
    private const string ProviderName = "Stripe";

    private readonly HttpClient _httpClient;
    private readonly StripeOptions _options;
    private readonly ILogger<StripeGateway> _logger;

    public StripeGateway(
        HttpClient httpClient,
        IOptions<StripeOptions> options,
        ILogger<StripeGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentGatewayChargeResult> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken)
    {
        if (!IsStripeProvider(request.Provider))
        {
            return UnsupportedCharge(request);
        }

        Dictionary<string, string> payload = new(StringComparer.Ordinal)
        {
            ["amount"] = ToMinorUnits(request.Amount).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["currency"] = request.Currency.ToLowerInvariant(),
            ["confirm"] = "false",
            ["metadata[invoice_id]"] = request.InvoiceId.ToString(),
            ["metadata[reference]"] = request.Reference,
            ["description"] = $"Invoice payment {request.Reference}",
        };

        using HttpRequestMessage message = new(HttpMethod.Post, "/v1/payment_intents")
        {
            Content = new FormUrlEncodedContent(payload),
        };

        JsonDocument? document = await SendForJsonAsync(message, cancellationToken);
        if (document is null)
        {
            return new PaymentGatewayChargeResult(false, ProviderName, string.Empty, request.Reference, "Error", null, "Failed to create Stripe payment intent.");
        }

        string transactionId = document.RootElement.TryGetProperty("id", out JsonElement idEl) ? idEl.GetString() ?? string.Empty : string.Empty;
        string status = document.RootElement.TryGetProperty("status", out JsonElement statusEl) ? statusEl.GetString() ?? "requires_payment_method" : "requires_payment_method";
        string? redirectUrl = null;

        return new PaymentGatewayChargeResult(
            IsSuccessful: !string.IsNullOrWhiteSpace(transactionId),
            Provider: ProviderName,
            TransactionId: transactionId,
            Reference: request.Reference,
            Status: status,
            RedirectUrl: redirectUrl,
            FailureReason: string.IsNullOrWhiteSpace(transactionId) ? "Missing payment intent id from Stripe response." : null);
    }

    public async Task<PaymentGatewayVerificationResult> VerifyAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken)
    {
        if (!IsStripeProvider(request.Provider))
        {
            return UnsupportedVerification(request);
        }

        using HttpRequestMessage message = new(HttpMethod.Get, $"/v1/payment_intents/{Uri.EscapeDataString(request.TransactionId)}");
        JsonDocument? document = await SendForJsonAsync(message, cancellationToken);
        if (document is null)
        {
            return new PaymentGatewayVerificationResult(false, ProviderName, request.TransactionId, request.Reference, "Error", null, null, null, "Failed to verify Stripe payment intent.");
        }

        JsonElement root = document.RootElement;
        string status = root.TryGetProperty("status", out JsonElement statusEl) ? statusEl.GetString() ?? "unknown" : "unknown";
        decimal? amount = root.TryGetProperty("amount_received", out JsonElement amountEl) && amountEl.TryGetDecimal(out decimal minorUnits)
            ? FromMinorUnits(minorUnits)
            : null;
        string? currency = root.TryGetProperty("currency", out JsonElement currencyEl) ? currencyEl.GetString()?.ToUpperInvariant() : null;

        bool isSuccessful = status is "succeeded" or "processing";
        return new PaymentGatewayVerificationResult(
            IsSuccessful: isSuccessful,
            Provider: ProviderName,
            TransactionId: request.TransactionId,
            Reference: request.Reference,
            Status: status,
            Amount: amount,
            Currency: currency,
            ProcessedAtUtc: DateTime.UtcNow,
            FailureReason: isSuccessful ? null : "Stripe payment intent is not in a successful state.");
    }

    public async Task<PaymentGatewayRefundResult> RefundAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken)
    {
        if (!IsStripeProvider(request.Provider))
        {
            return UnsupportedRefund(request);
        }

        Dictionary<string, string> payload = new(StringComparer.Ordinal)
        {
            ["payment_intent"] = request.TransactionId,
            ["amount"] = ToMinorUnits(request.Amount).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["reason"] = "requested_by_customer",
        };

        using HttpRequestMessage message = new(HttpMethod.Post, "/v1/refunds")
        {
            Content = new FormUrlEncodedContent(payload),
        };

        JsonDocument? document = await SendForJsonAsync(message, cancellationToken);
        if (document is null)
        {
            return new PaymentGatewayRefundResult(false, ProviderName, request.TransactionId, "Error", "Failed to create Stripe refund.");
        }

        string status = document.RootElement.TryGetProperty("status", out JsonElement statusEl) ? statusEl.GetString() ?? "unknown" : "unknown";
        bool isSuccessful = status is "succeeded" or "pending";
        return new PaymentGatewayRefundResult(
            IsSuccessful: isSuccessful,
            Provider: ProviderName,
            TransactionId: request.TransactionId,
            Status: status,
            FailureReason: isSuccessful ? null : "Stripe refund is not in a successful state.");
    }

    private async Task<JsonDocument?> SendForJsonAsync(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.SecretKey) && _httpClient.DefaultRequestHeaders.Authorization is null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.SecretKey);
        }

        using HttpResponseMessage response = await _httpClient.SendAsync(message, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Stripe API returned {StatusCode}: {Body}", response.StatusCode, body);
            return null;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        return JsonDocument.Parse(body);
    }

    private static bool IsStripeProvider(string provider) =>
        provider.Equals("stripe", StringComparison.OrdinalIgnoreCase);

    private static long ToMinorUnits(decimal amount) => (long)decimal.Round(amount * 100m, 0, MidpointRounding.ToEven);

    private static decimal FromMinorUnits(decimal amount) => decimal.Round(amount / 100m, 2, MidpointRounding.ToEven);

    private static PaymentGatewayChargeResult UnsupportedCharge(PaymentGatewayChargeRequest request) =>
        new(false, ProviderName, string.Empty, request.Reference, "UnsupportedProvider", null, "Unsupported provider for Stripe gateway.");

    private static PaymentGatewayVerificationResult UnsupportedVerification(PaymentGatewayVerificationRequest request) =>
        new(false, ProviderName, request.TransactionId, request.Reference, "UnsupportedProvider", null, null, null, "Unsupported provider for Stripe gateway.");

    private static PaymentGatewayRefundResult UnsupportedRefund(PaymentGatewayRefundRequest request) =>
        new(false, ProviderName, request.TransactionId, "UnsupportedProvider", "Unsupported provider for Stripe gateway.");
}
