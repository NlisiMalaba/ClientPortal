using Application.Invoices.Abstractions;

namespace Infrastructure.Invoices;

public sealed class ManualPaymentGateway : IPaymentGateway
{
    private const string ProviderName = "Manual";

    public Task<PaymentGatewayChargeResult> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsManualProvider(request.Provider))
        {
            return Task.FromResult(UnsupportedCharge(request));
        }

        PaymentGatewayChargeResult result = new(
            IsSuccessful: true,
            Provider: ProviderName,
            TransactionId: BuildManualTransactionId(request.Reference),
            Reference: request.Reference,
            Status: "PendingManualSettlement",
            RedirectUrl: null,
            FailureReason: null);

        return Task.FromResult(result);
    }

    public Task<PaymentGatewayVerificationResult> VerifyAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsManualProvider(request.Provider))
        {
            return Task.FromResult(UnsupportedVerification(request));
        }

        PaymentGatewayVerificationResult result = new(
            IsSuccessful: true,
            Provider: ProviderName,
            TransactionId: request.TransactionId,
            Reference: request.Reference,
            Status: "ManuallyVerified",
            Amount: null,
            Currency: null,
            ProcessedAtUtc: DateTime.UtcNow,
            FailureReason: null);

        return Task.FromResult(result);
    }

    public Task<PaymentGatewayRefundResult> RefundAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsManualProvider(request.Provider))
        {
            return Task.FromResult(UnsupportedRefund(request));
        }

        PaymentGatewayRefundResult result = new(
            IsSuccessful: true,
            Provider: ProviderName,
            TransactionId: request.TransactionId,
            Status: "RefundRecordedManually",
            FailureReason: null);

        return Task.FromResult(result);
    }

    private static bool IsManualProvider(string provider)
    {
        return provider.Equals("manual", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("eft", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("banktransfer", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("bank-transfer", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("ecocash", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("mpesa", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("m-pesa", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildManualTransactionId(string reference)
    {
        string normalizedReference = string.IsNullOrWhiteSpace(reference) ? "manual" : reference.Trim();
        return $"manual-{normalizedReference}-{Guid.CreateVersion7():N}";
    }

    private static PaymentGatewayChargeResult UnsupportedCharge(PaymentGatewayChargeRequest request) =>
        new(false, ProviderName, string.Empty, request.Reference, "UnsupportedProvider", null, "Unsupported provider for manual gateway.");

    private static PaymentGatewayVerificationResult UnsupportedVerification(PaymentGatewayVerificationRequest request) =>
        new(false, ProviderName, request.TransactionId, request.Reference, "UnsupportedProvider", null, null, null, "Unsupported provider for manual gateway.");

    private static PaymentGatewayRefundResult UnsupportedRefund(PaymentGatewayRefundRequest request) =>
        new(false, ProviderName, request.TransactionId, "UnsupportedProvider", "Unsupported provider for manual gateway.");
}
