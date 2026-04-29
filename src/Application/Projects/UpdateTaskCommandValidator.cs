using FluentValidation;

namespace Application.Projects;

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(command => command.TaskId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(command => command.AssigneeId)
            .NotEmpty();

        RuleFor(command => command.DueDate)
            .NotEqual(default(DateOnly));
    }
}
