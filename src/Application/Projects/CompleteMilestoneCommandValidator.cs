using FluentValidation;

namespace Application.Projects;

public sealed class CompleteMilestoneCommandValidator : AbstractValidator<CompleteMilestoneCommand>
{
    public CompleteMilestoneCommandValidator()
    {
        RuleFor(command => command.MilestoneId)
            .NotEmpty();
    }
}
