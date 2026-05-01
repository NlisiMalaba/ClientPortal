using Application.Documents.Abstractions;
using Application.Documents.Dtos;
using Domain;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace Infrastructure.Persistence;

public sealed class ContractRepository : IContractRepository
{
    private readonly TenantDbContext _tenantDbContext;

    public ContractRepository(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

    public Task<Contract?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _tenantDbContext.Set<Contract>().SingleOrDefaultAsync(contract => contract.Id == id, cancellationToken);
    }

    public async Task<PagedResult<ContractListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? clientId,
        ContractStatus? status,
        CancellationToken cancellationToken)
    {
        IQueryable<Contract> query = _tenantDbContext.Set<Contract>().AsNoTracking();

        if (clientId.HasValue)
        {
            query = query.Where(contract => contract.ClientId == clientId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(contract => contract.Status == status.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        IReadOnlyList<ContractListItemDto> items = await query
            .OrderByDescending(contract => contract.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(contract => new ContractListItemDto(
                contract.Id,
                contract.ClientId,
                contract.Title,
                contract.Status,
                contract.SignedAt,
                contract.ExpiresAt,
                contract.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ContractListItemDto>(items, totalCount, page, pageSize);
    }

    public void Add(Contract contract)
    {
        _tenantDbContext.Set<Contract>().Add(contract);
    }

    public void Update(Contract contract)
    {
        _tenantDbContext.Set<Contract>().Update(contract);
    }
}
