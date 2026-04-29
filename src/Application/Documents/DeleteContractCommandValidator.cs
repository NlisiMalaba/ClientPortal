using FluentValidation;

namespace Application.Documents;

public sealed class DeleteContractCommandValidator : AbstractValidator<DeleteContractCommand>
{
    public DeleteContractCommandValidator()
    {
        RuleFor(command => command.ContractId).NotEmpty();
        RuleFor(command => command.ClientId).NotEmpty();
    }
}
