using Application.Abstractions;
using Application.Invoices.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed class ProcessGatewayPaymentCommandHandler : IRequestHandler<ProcessGatewayPaymentCommand, Result>
{
    private static readonly Error InvalidWebhookSignatureError = new(
        "Invoices.InvalidWebhookSignature",
        "Webhook signature verification failed.",
        ErrorType.Forbidden);

    private static readonly Error InvoiceNotFoundError = new(
        "Invoices.NotFound",
        "Invoice was not found.",
        ErrorType.NotFound);

    private static readonly Error CurrencyMismatchError = new(
        "Invoices.PaymentCurrencyMismatch",
        "Payment currency must match invoice currency.",
        ErrorType.Validation);

    private static readonly Error DuplicateGatewayPaymentError = new(
        "Invoices.DuplicateGatewayPayment",
        "Gateway payment reference has already been processed.",
        ErrorType.Conflict);

    private static readonly Error InvalidPaymentError = new(
        "Invoices.InvalidPayment",
        "Payment could not be applied to this invoice.",
        ErrorType.Conflict);

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayWebhookVerifier _paymentGatewayWebhookVerifier;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessGatewayPaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        IPaymentGatewayWebhookVerifier paymentGatewayWebhookVerifier,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _paymentGatewayWebhookVerifier = paymentGatewayWebhookVerifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ProcessGatewayPaymentCommand request, CancellationToken cancellationToken)
    {
        PaymentGatewayWebhookVerificationResult verification = await _paymentGatewayWebhookVerifier.VerifyAsync(
            request.Provider,
            request.Payload,
            request.Signature,
            cancellationToken);

        if (!verification.IsValid)
        {
            return Result.Failure(InvalidWebhookSignatureError);
        }

        Invoice? invoice = await _invoiceRepository.FindByIdAsync(verification.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result.Failure(InvoiceNotFoundError);
        }

        string normalizedCurrency = verification.Currency.Trim().ToUpperInvariant();
        if (!string.Equals(invoice.Currency, normalizedCurrency, StringComparison.Ordinal))
        {
            return Result.Failure(CurrencyMismatchError);
        }

        bool exists = await _paymentRepository.ExistsByReferenceAsync(invoice.Id, verification.Reference, cancellationToken);
        if (exists)
        {
            return Result.Failure(DuplicateGatewayPaymentError);
        }

        Payment payment;
        try
        {
            invoice.RecordPayment(verification.Amount, verification.PaidAtUtc);
            payment = Payment.Create(
                id: Guid.CreateVersion7(),
                invoiceId: invoice.Id,
                amount: verification.Amount,
                currency: normalizedCurrency,
                method: verification.Method,
                reference: verification.Reference,
                paidAtUtc: verification.PaidAtUtc);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result.Failure(InvalidPaymentError);
        }

        _paymentRepository.Add(payment);
        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
