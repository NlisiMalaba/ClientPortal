using Application.Invoices.Abstractions;

namespace Infrastructure.Invoices;

public sealed class RoutedPaymentGateway : IPaymentGateway
{
    private readonly PeachPaymentsGateway _peachGateway;
    private readonly StripeGateway _stripeGateway;
    private readonly ManualPaymentGateway _manualGateway;
    private readonly NoopPaymentGateway _noopGateway;

    public RoutedPaymentGateway(
        PeachPaymentsGateway peachGateway,
        StripeGateway stripeGateway,
        ManualPaymentGateway manualGateway,
        NoopPaymentGateway noopGateway)
    {
        _peachGateway = peachGateway;
        _stripeGateway = stripeGateway;
        _manualGateway = manualGateway;
        _noopGateway = noopGateway;
    }

    public Task<PaymentGatewayChargeResult> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken)
    {
        return ResolveGateway(request.Provider).ChargeAsync(request, cancellationToken);
    }

    public Task<PaymentGatewayVerificationResult> VerifyAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken)
    {
        return ResolveGateway(request.Provider).VerifyAsync(request, cancellationToken);
    }

    public Task<PaymentGatewayRefundResult> RefundAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken)
    {
        return ResolveGateway(request.Provider).RefundAsync(request, cancellationToken);
    }

    private IPaymentGateway ResolveGateway(string provider)
    {
        if (provider.Equals("peach", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("peachpayments", StringComparison.OrdinalIgnoreCase))
        {
            return _peachGateway;
        }

        if (provider.Equals("stripe", StringComparison.OrdinalIgnoreCase))
        {
            return _stripeGateway;
        }

        if (provider.Equals("manual", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("eft", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("banktransfer", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("bank-transfer", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("ecocash", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("mpesa", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("m-pesa", StringComparison.OrdinalIgnoreCase))
        {
            return _manualGateway;
        }

        return _noopGateway;
    }
}
