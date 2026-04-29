using FluentValidation;

namespace Application.Projects;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.MilestoneId)
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
