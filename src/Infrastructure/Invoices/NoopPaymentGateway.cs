using Application.Invoices.Abstractions;

namespace Infrastructure.Invoices;

public sealed class NoopPaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayChargeResult> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PaymentGatewayChargeResult result = new(
            IsSuccessful: false,
            Provider: request.Provider,
            TransactionId: string.Empty,
            Reference: request.Reference,
            Status: "NotConfigured",
            RedirectUrl: null,
            FailureReason: "No payment gateway provider has been configured.");

        return Task.FromResult(result);
    }

    public Task<PaymentGatewayVerificationResult> VerifyAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PaymentGatewayVerificationResult result = new(
            IsSuccessful: false,
            Provider: request.Provider,
            TransactionId: request.TransactionId,
            Reference: request.Reference,
            Status: "NotConfigured",
            Amount: null,
            Currency: null,
            ProcessedAtUtc: null,
            FailureReason: "No payment gateway provider has been configured.");

        return Task.FromResult(result);
    }

    public Task<PaymentGatewayRefundResult> RefundAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PaymentGatewayRefundResult result = new(
            IsSuccessful: false,
            Provider: request.Provider,
            TransactionId: request.TransactionId,
            Status: "NotConfigured",
            FailureReason: "No payment gateway provider has been configured.");

        return Task.FromResult(result);
    }
}
