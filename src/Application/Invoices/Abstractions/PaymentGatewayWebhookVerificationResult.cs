namespace Application.Invoices.Abstractions;

public sealed record PaymentGatewayWebhookVerificationResult(
    bool IsValid,
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string Reference,
    string Method,
    DateTime PaidAtUtc);
