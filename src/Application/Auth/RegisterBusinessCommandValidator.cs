using FluentValidation;

namespace Application.Auth;

public sealed class RegisterBusinessCommandValidator : AbstractValidator<RegisterBusinessCommand>
{
    public RegisterBusinessCommandValidator()
    {
        RuleFor(command => command.CompanyName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(command => command.TenantSlug)
            .NotEmpty()
            .MaximumLength(128)
            .Matches("^[a-z0-9-]+$");

        RuleFor(command => command.CompanyDomain)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(command => command.OwnerFullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.OwnerEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.OwnerPassword)
            .NotEmpty()
            .MinimumLength(8);
    }
}
