using Application.Abstractions;
using Application.Documents.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Documents;

public sealed class DeleteContractCommandHandler : IRequestHandler<DeleteContractCommand, Result>
{
    private static readonly Error ContractNotFoundError = new(
        "Contracts.NotFound",
        "Contract was not found.",
        ErrorType.NotFound);

    private static readonly Error ContractInvalidStateError = new(
        "Contracts.InvalidState",
        "Contract cannot be cancelled in its current state.",
        ErrorType.Conflict);

    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteContractCommandHandler(IContractRepository contractRepository, IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteContractCommand request, CancellationToken cancellationToken)
    {
        Contract? contract = await _contractRepository.FindByIdAsync(request.ContractId, cancellationToken);
        if (contract is null || contract.ClientId != request.ClientId)
        {
            return Result.Failure(ContractNotFoundError);
        }

        try
        {
            contract.Cancel();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(ContractInvalidStateError);
        }

        _contractRepository.Update(contract);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
