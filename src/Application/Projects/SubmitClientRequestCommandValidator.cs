using FluentValidation;

namespace Application.Projects;

public sealed class SubmitClientRequestCommandValidator : AbstractValidator<SubmitClientRequestCommand>
{
    public SubmitClientRequestCommandValidator()
    {
        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
