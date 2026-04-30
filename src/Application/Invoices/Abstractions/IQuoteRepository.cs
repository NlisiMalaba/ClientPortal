using Domain;

namespace Application.Invoices.Abstractions;

public interface IQuoteRepository
{
    Task<Quote?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    void Update(Quote quote);
}
