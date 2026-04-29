using FluentValidation;

namespace Application.Projects;

public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(command => command.TaskId)
            .NotEmpty();
    }
}
