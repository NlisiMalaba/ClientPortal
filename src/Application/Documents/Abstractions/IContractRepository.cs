using Application.Documents.Dtos;
using Domain;
using Shared;

namespace Application.Documents.Abstractions;

public interface IContractRepository
{
    Task<Contract?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ContractListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? clientId,
        ContractStatus? status,
        CancellationToken cancellationToken);

    void Add(Contract contract);

    void Update(Contract contract);
}
