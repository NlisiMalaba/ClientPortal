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

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendInvoiceCommandHandler> _logger;

    public SendInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendInvoiceCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
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

        return Result.Success();
    }
}
