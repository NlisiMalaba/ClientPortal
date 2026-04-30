using Application.Abstractions;
using Application.Invoices.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Invoices;

public sealed class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, Result>
{
    private static readonly Error InvoiceNotFoundError = new(
        "Invoices.NotFound",
        "Invoice was not found.",
        ErrorType.NotFound);

    private static readonly Error InvoiceNotDraftError = new(
        "Invoices.NotDraft",
        "Only draft invoices can be updated.",
        ErrorType.Conflict);

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        Invoice? invoice = await _invoiceRepository.FindByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice is null || invoice.ClientId != request.ClientId)
        {
            return Result.Failure(InvoiceNotFoundError);
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            return Result.Failure(InvoiceNotDraftError);
        }

        invoice.ReplaceLineItems(request.LineItems.Select(item => new LineItem(
            item.Description,
            item.Quantity,
            item.UnitPrice,
            item.TaxRate)));
        invoice.UpdateInvoiceNumber(request.InvoiceNumber);
        invoice.UpdateCurrency(request.Currency);
        invoice.UpdateNotes(request.Notes);
        invoice.SetDueDate(request.DueDate);

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
