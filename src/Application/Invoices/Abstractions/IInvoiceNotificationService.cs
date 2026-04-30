using Domain;

namespace Application.Invoices.Abstractions;

public interface IInvoiceNotificationService
{
    Task NotifyInvoiceSentAsync(
        Client client,
        Invoice invoice,
        InvoicePdfDocument invoicePdf,
        CancellationToken cancellationToken);
}
