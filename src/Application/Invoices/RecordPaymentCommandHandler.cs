using Application.Abstractions;
using Application.Invoices.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result>
{
    private static readonly Error InvoiceNotFoundError = new(
        "Invoices.NotFound",
        "Invoice was not found.",
        ErrorType.NotFound);

    private static readonly Error CurrencyMismatchError = new(
        "Invoices.PaymentCurrencyMismatch",
        "Payment currency must match invoice currency.",
        ErrorType.Validation);

    private static readonly Error PaymentInvalidError = new(
        "Invoices.InvalidPayment",
        "Payment could not be recorded for this invoice.",
        ErrorType.Conflict);

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        Invoice? invoice = await _invoiceRepository.FindByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice is null || invoice.ClientId != request.ClientId)
        {
            return Result.Failure(InvoiceNotFoundError);
        }

        string normalizedCurrency = request.Currency.Trim().ToUpperInvariant();
        if (!string.Equals(invoice.Currency, normalizedCurrency, StringComparison.Ordinal))
        {
            return Result.Failure(CurrencyMismatchError);
        }

        Payment payment;
        try
        {
            invoice.RecordPayment(request.Amount, request.PaidAtUtc);
            payment = Payment.Create(
                id: Guid.CreateVersion7(),
                invoiceId: invoice.Id,
                amount: request.Amount,
                currency: request.Currency,
                method: request.Method,
                reference: request.Reference,
                paidAtUtc: request.PaidAtUtc,
                notes: request.Notes);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(PaymentInvalidError);
        }

        _paymentRepository.Add(payment);
        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
