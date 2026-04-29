using FluentValidation;

namespace Application.Clients;

public sealed class DeactivateClientCommandValidator : AbstractValidator<DeactivateClientCommand>
{
    public DeactivateClientCommandValidator()
    {
        RuleFor(command => command.ClientId)
            .NotEmpty();
    }
}
