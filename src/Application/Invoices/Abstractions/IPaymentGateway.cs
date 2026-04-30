namespace Application.Invoices.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentGatewayChargeResult> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken);

    Task<PaymentGatewayVerificationResult> VerifyAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken);

    Task<PaymentGatewayRefundResult> RefundAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken);
}

public sealed record PaymentGatewayChargeRequest(
    string Provider,
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string Reference,
    string CallbackUrl);

public sealed record PaymentGatewayChargeResult(
    bool IsSuccessful,
    string Provider,
    string TransactionId,
    string Reference,
    string Status,
    string? RedirectUrl,
    string? FailureReason);

public sealed record PaymentGatewayVerificationRequest(
    string Provider,
    string TransactionId,
    string Reference);

public sealed record PaymentGatewayVerificationResult(
    bool IsSuccessful,
    string Provider,
    string TransactionId,
    string Reference,
    string Status,
    decimal? Amount,
    string? Currency,
    DateTime? ProcessedAtUtc,
    string? FailureReason);

public sealed record PaymentGatewayRefundRequest(
    string Provider,
    string TransactionId,
    decimal Amount,
    string Currency,
    string Reason);

public sealed record PaymentGatewayRefundResult(
    bool IsSuccessful,
    string Provider,
    string TransactionId,
    string Status,
    string? FailureReason);
