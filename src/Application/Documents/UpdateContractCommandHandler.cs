using Application.Abstractions;
using Application.Documents.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, Result>
{
    private static readonly Error ContractNotFoundError = new(
        "Contracts.NotFound",
        "Contract was not found.",
        ErrorType.NotFound);

    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateContractCommandHandler(IContractRepository contractRepository, IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
    {
        Contract? contract = await _contractRepository.FindByIdAsync(request.ContractId, cancellationToken);
        if (contract is null || contract.ClientId != request.ClientId)
        {
            return Result.Failure(ContractNotFoundError);
        }

        contract.Rename(request.Title);
        contract.ReplaceS3Key(request.S3Key);
        contract.ReplaceParties(request.Parties);
        contract.SetExpiry(request.ExpiresAtUtc);

        _contractRepository.Update(contract);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
