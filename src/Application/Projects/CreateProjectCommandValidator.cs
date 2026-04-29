using FluentValidation;

namespace Application.Projects;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(command => command.StartDate)
            .NotEqual(default(DateOnly));

        RuleFor(command => command.EndDate)
            .NotEqual(default(DateOnly))
            .GreaterThanOrEqualTo(command => command.StartDate);

        RuleFor(command => command.Budget)
            .GreaterThanOrEqualTo(0m);

        RuleFor(command => command.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$");

        RuleForEach(command => command.Milestones)
            .SetValidator(new CreateProjectMilestoneScaffoldItemValidator());
    }

    private sealed class CreateProjectMilestoneScaffoldItemValidator : AbstractValidator<CreateProjectMilestoneScaffoldItem>
    {
        public CreateProjectMilestoneScaffoldItemValidator()
        {
            RuleFor(item => item.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(item => item.DueDate)
                .NotEqual(default(DateOnly));
        }
    }
}
