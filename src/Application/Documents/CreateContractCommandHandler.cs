using Application.Abstractions;
using Application.Documents.Abstractions;
using Application.Documents.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, Result<ContractDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateContractCommandHandler(IContractRepository contractRepository, IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContractDto>> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        Contract contract = Contract.Create(
            id: Guid.CreateVersion7(),
            clientId: request.ClientId,
            title: request.Title,
            s3Key: request.S3Key,
            parties: request.Parties,
            expiresAtUtc: request.ExpiresAtUtc);

        _contractRepository.Add(contract);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ContractDto>.Success(Map(contract));
    }

    private static ContractDto Map(Contract contract)
    {
        return new ContractDto(
            contract.Id,
            contract.ClientId,
            contract.Title,
            contract.Status,
            contract.SignedAt,
            contract.ExpiresAt,
            contract.S3Key,
            contract.Parties,
            contract.CreatedAt,
            contract.UpdatedAt);
    }
}
