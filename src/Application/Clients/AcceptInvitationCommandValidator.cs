using FluentValidation;

namespace Application.Clients;

public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(command => command.Token)
            .NotEmpty()
            .MinimumLength(16);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
