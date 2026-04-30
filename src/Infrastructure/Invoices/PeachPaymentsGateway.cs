using System.Net.Http.Headers;
using System.Text.Json;
using Application.Invoices.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Invoices;

public sealed class PeachPaymentsGateway : IPaymentGateway
{
    private const string ProviderName = "PeachPayments";

    private readonly HttpClient _httpClient;
    private readonly PeachPaymentsOptions _options;
    private readonly ILogger<PeachPaymentsGateway> _logger;

    public PeachPaymentsGateway(
        HttpClient httpClient,
        IOptions<PeachPaymentsOptions> options,
        ILogger<PeachPaymentsGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentGatewayChargeResult> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken)
    {
        if (!IsPeachProvider(request.Provider))
        {
            return UnsupportedCharge(request);
        }

        Dictionary<string, string> payload = new(StringComparer.Ordinal)
        {
            ["entityId"] = _options.EntityId,
            ["amount"] = decimal.Round(request.Amount, 2, MidpointRounding.ToEven).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            ["currency"] = request.Currency,
            ["paymentType"] = "DB",
            ["merchantTransactionId"] = request.Reference,
            ["shopperResultUrl"] = request.CallbackUrl,
        };

        using HttpRequestMessage message = new(HttpMethod.Post, "/v1/checkouts")
        {
            Content = new FormUrlEncodedContent(payload),
        };

        JsonDocument? document = await SendForJsonAsync(message, cancellationToken);
        if (document is null)
        {
            return new PaymentGatewayChargeResult(
                IsSuccessful: false,
                Provider: ProviderName,
                TransactionId: string.Empty,
                Reference: request.Reference,
                Status: "Error",
                RedirectUrl: null,
                FailureReason: "Failed to create Peach checkout session.");
        }

        JsonElement root = document.RootElement;
        string checkoutId = root.TryGetProperty("id", out JsonElement idEl) ? idEl.GetString() ?? string.Empty : string.Empty;
        string? redirectUrl = root.TryGetProperty("redirect", out JsonElement redirectEl) ? redirectEl.GetString() : null;

        return new PaymentGatewayChargeResult(
            IsSuccessful: !string.IsNullOrWhiteSpace(checkoutId),
            Provider: ProviderName,
            TransactionId: checkoutId,
            Reference: request.Reference,
            Status: "Pending",
            RedirectUrl: redirectUrl,
            FailureReason: string.IsNullOrWhiteSpace(checkoutId) ? "Missing checkout id from Peach response." : null);
    }

    public async Task<PaymentGatewayVerificationResult> VerifyAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken)
    {
        if (!IsPeachProvider(request.Provider))
        {
            return UnsupportedVerification(request);
        }

        string relativeUrl = $"/v1/checkouts/{Uri.EscapeDataString(request.TransactionId)}/payment?entityId={Uri.EscapeDataString(_options.EntityId)}";
        using HttpRequestMessage message = new(HttpMethod.Get, relativeUrl);

        JsonDocument? document = await SendForJsonAsync(message, cancellationToken);
        if (document is null)
        {
            return new PaymentGatewayVerificationResult(
                IsSuccessful: false,
                Provider: ProviderName,
                TransactionId: request.TransactionId,
                Reference: request.Reference,
                Status: "Error",
                Amount: null,
                Currency: null,
                ProcessedAtUtc: null,
                FailureReason: "Failed to verify Peach transaction.");
        }

        JsonElement root = document.RootElement;
        string status = root.TryGetProperty("result", out JsonElement resultEl) &&
                        resultEl.TryGetProperty("code", out JsonElement codeEl)
            ? codeEl.GetString() ?? "Unknown"
            : "Unknown";

        decimal? amount = root.TryGetProperty("amount", out JsonElement amountEl) &&
                          decimal.TryParse(amountEl.GetString(), out decimal parsedAmount)
            ? parsedAmount
            : null;

        string? currency = root.TryGetProperty("currency", out JsonElement currencyEl) ? currencyEl.GetString() : null;

        return new PaymentGatewayVerificationResult(
            IsSuccessful: status.StartsWith("000.", StringComparison.Ordinal),
            Provider: ProviderName,
            TransactionId: request.TransactionId,
            Reference: request.Reference,
            Status: status,
            Amount: amount,
            Currency: currency,
            ProcessedAtUtc: DateTime.UtcNow,
            FailureReason: status.StartsWith("000.", StringComparison.Ordinal) ? null : "Peach payment verification returned non-success code.");
    }

    public async Task<PaymentGatewayRefundResult> RefundAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken)
    {
        if (!IsPeachProvider(request.Provider))
        {
            return UnsupportedRefund(request);
        }

        Dictionary<string, string> payload = new(StringComparer.Ordinal)
        {
            ["entityId"] = _options.EntityId,
            ["amount"] = decimal.Round(request.Amount, 2, MidpointRounding.ToEven).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            ["currency"] = request.Currency,
            ["paymentType"] = "RF",
        };

        string relativeUrl = $"/v1/payments/{Uri.EscapeDataString(request.TransactionId)}";
        using HttpRequestMessage message = new(HttpMethod.Post, relativeUrl)
        {
            Content = new FormUrlEncodedContent(payload),
        };

        JsonDocument? document = await SendForJsonAsync(message, cancellationToken);
        if (document is null)
        {
            return new PaymentGatewayRefundResult(
                IsSuccessful: false,
                Provider: ProviderName,
                TransactionId: request.TransactionId,
                Status: "Error",
                FailureReason: "Failed to issue Peach refund.");
        }

        string status = document.RootElement.TryGetProperty("result", out JsonElement resultEl) &&
                        resultEl.TryGetProperty("code", out JsonElement codeEl)
            ? codeEl.GetString() ?? "Unknown"
            : "Unknown";

        return new PaymentGatewayRefundResult(
            IsSuccessful: status.StartsWith("000.", StringComparison.Ordinal),
            Provider: ProviderName,
            TransactionId: request.TransactionId,
            Status: status,
            FailureReason: status.StartsWith("000.", StringComparison.Ordinal) ? null : "Peach refund returned non-success code.");
    }

    private async Task<JsonDocument?> SendForJsonAsync(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.SendAsync(message, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Peach API returned {StatusCode}: {Body}", response.StatusCode, body);
            return null;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        return JsonDocument.Parse(body);
    }

    private static bool IsPeachProvider(string provider)
    {
        return provider.Equals("peach", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("peachpayments", StringComparison.OrdinalIgnoreCase);
    }

    private static PaymentGatewayChargeResult UnsupportedCharge(PaymentGatewayChargeRequest request) =>
        new(false, ProviderName, string.Empty, request.Reference, "UnsupportedProvider", null, "Unsupported provider for Peach gateway.");

    private static PaymentGatewayVerificationResult UnsupportedVerification(PaymentGatewayVerificationRequest request) =>
        new(false, ProviderName, request.TransactionId, request.Reference, "UnsupportedProvider", null, null, null, "Unsupported provider for Peach gateway.");

    private static PaymentGatewayRefundResult UnsupportedRefund(PaymentGatewayRefundRequest request) =>
        new(false, ProviderName, request.TransactionId, "UnsupportedProvider", "Unsupported provider for Peach gateway.");
}
