using Application.Documents.Abstractions;
using Application.Documents.Dtos;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, Result<PagedResult<ContractListItemDto>>>
{
    private readonly IContractRepository _contractRepository;

    public GetContractsQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<Result<PagedResult<ContractListItemDto>>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
    {
        PagedResult<ContractListItemDto> contracts = await _contractRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.ClientId,
            request.Status,
            cancellationToken);

        return Result<PagedResult<ContractListItemDto>>.Success(contracts);
    }
}
