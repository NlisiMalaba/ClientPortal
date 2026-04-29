using FluentValidation;

namespace Application.Documents;

public sealed class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(command => command.ClientId).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(256);
        RuleFor(command => command.S3Key).NotEmpty().MaximumLength(2048);
        RuleForEach(command => command.Parties!).NotEmpty().MaximumLength(256).When(command => command.Parties is not null);
    }
}
