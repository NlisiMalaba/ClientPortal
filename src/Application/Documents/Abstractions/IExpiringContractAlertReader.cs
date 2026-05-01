namespace Application.Documents.Abstractions;

public interface IExpiringContractAlertReader
{
    Task<IReadOnlyList<ExpiringContractAlertItem>> GetExpiringContractsAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken);
}
