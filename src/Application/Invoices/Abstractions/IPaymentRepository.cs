using Domain;

namespace Application.Invoices.Abstractions;

public interface IPaymentRepository
{
    Task<bool> ExistsByReferenceAsync(Guid invoiceId, string reference, CancellationToken cancellationToken);

    void Add(Payment payment);
}
