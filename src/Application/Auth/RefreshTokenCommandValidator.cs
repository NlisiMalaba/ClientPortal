using FluentValidation;

namespace Application.Auth;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty();

        RuleFor(command => command.ClientIp)
            .NotEmpty();
    }
}
