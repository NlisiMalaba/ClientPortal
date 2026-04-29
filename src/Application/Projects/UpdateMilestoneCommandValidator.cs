using FluentValidation;
using Domain;

namespace Application.Projects;

public sealed class UpdateMilestoneCommandValidator : AbstractValidator<UpdateMilestoneCommand>
{
    public UpdateMilestoneCommandValidator()
    {
        RuleFor(command => command.MilestoneId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.DueDate)
            .NotEqual(default(DateOnly));

        RuleFor(command => command.CompletedAtUtc)
            .NotNull()
            .When(command => command.Status == MilestoneStatus.Completed);

        RuleFor(command => command.CompletedAtUtc)
            .Null()
            .When(command => command.Status == MilestoneStatus.Planned);
    }
}
