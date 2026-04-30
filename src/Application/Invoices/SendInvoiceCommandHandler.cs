using Application.Abstractions;
using Application.Clients.Abstractions;
using Application.Invoices.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Invoices;

public sealed class SendInvoiceCommandHandler : IRequestHandler<SendInvoiceCommand, Result>
{
    private static readonly Error InvoiceNotFoundError = new(
        "Invoices.NotFound",
        "Invoice was not found.",
        ErrorType.NotFound);

    private static readonly Error ClientNotFoundError = new(
        "Clients.NotFound",
        "Client was not found.",
        ErrorType.NotFound);

    private static readonly Error InvoiceInvalidStateError = new(
        "Invoices.InvalidState",
        "Invoice cannot be sent in its current state.",
        ErrorType.Conflict);

    private static readonly Error InvoicePdfGenerationFailedError = new(
        "Invoices.PdfGenerationFailed",
        "Invoice was sent but PDF generation failed.",
        ErrorType.Unexpected);

    private static readonly Error InvoiceNotificationFailedError = new(
        "Invoices.NotificationFailed",
        "Invoice was sent but notifying the client failed.",
        ErrorType.Unexpected);

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IInvoicePdfGenerator _invoicePdfGenerator;
    private readonly IInvoiceNotificationService _invoiceNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendInvoiceCommandHandler> _logger;

    public SendInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IInvoicePdfGenerator invoicePdfGenerator,
        IInvoiceNotificationService invoiceNotificationService,
        IUnitOfWork unitOfWork,
        ILogger<SendInvoiceCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _invoicePdfGenerator = invoicePdfGenerator;
        _invoiceNotificationService = invoiceNotificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SendInvoiceCommand request, CancellationToken cancellationToken)
    {
        Invoice? invoice = await _invoiceRepository.FindByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice is null || invoice.ClientId != request.ClientId)
        {
            return Result.Failure(InvoiceNotFoundError);
        }

        Client? client = await _clientRepository.FindByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            return Result.Failure(ClientNotFoundError);
        }

        try
        {
            invoice.MarkSent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid state transition while sending invoice {InvoiceId}.", invoice.Id);
            return Result.Failure(InvoiceInvalidStateError);
        }

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvoicePdfDocument invoicePdf;
        try
        {
            invoicePdf = await _invoicePdfGenerator.GenerateAsync(invoice, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for invoice {InvoiceId}.", invoice.Id);
            return Result.Failure(InvoicePdfGenerationFailedError);
        }

        try
        {
            await _invoiceNotificationService.NotifyInvoiceSentAsync(
                client,
                invoice,
                invoicePdf,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invoice notification for invoice {InvoiceId}.", invoice.Id);
            return Result.Failure(InvoiceNotificationFailedError);
        }

        return Result.Success();
    }
}
