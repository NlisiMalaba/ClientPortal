namespace Application.Invoices.Abstractions;

public interface IPaymentGatewayWebhookVerifier
{
    Task<PaymentGatewayWebhookVerificationResult> VerifyAsync(
        string provider,
        string payload,
        string signature,
        CancellationToken cancellationToken);
}
