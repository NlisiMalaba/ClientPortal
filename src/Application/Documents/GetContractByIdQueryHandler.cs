using Application.Documents.Abstractions;
using Application.Documents.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, Result<ContractDto>>
{
    private static readonly Error ContractNotFoundError = new(
        "Contracts.NotFound",
        "Contract was not found.",
        ErrorType.NotFound);

    private readonly IContractRepository _contractRepository;

    public GetContractByIdQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<Result<ContractDto>> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        Contract? contract = await _contractRepository.FindByIdAsync(request.ContractId, cancellationToken);
        if (contract is null)
        {
            return Result<ContractDto>.Failure(ContractNotFoundError);
        }

        return Result<ContractDto>.Success(new ContractDto(
            contract.Id,
            contract.ClientId,
            contract.Title,
            contract.Status,
            contract.SignedAt,
            contract.ExpiresAt,
            contract.S3Key,
            contract.Parties,
            contract.CreatedAt,
            contract.UpdatedAt));
    }
}
