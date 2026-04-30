using Domain;

namespace Application.Invoices.Abstractions;

public interface IInvoicePdfGenerator
{
    Task<InvoicePdfDocument> GenerateAsync(
        Invoice invoice,
        CancellationToken cancellationToken);
}
