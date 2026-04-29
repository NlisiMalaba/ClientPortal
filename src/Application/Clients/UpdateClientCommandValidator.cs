using FluentValidation;

namespace Application.Clients;

public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.CompanyName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(command => command.ContactName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Phone)
            .NotEmpty()
            .Matches(@"^\+[1-9]\d{7,14}$");

        RuleFor(command => command.Notes)
            .MaximumLength(4000);
    }
}
