using FluentValidation;

namespace Application.Projects;

public sealed class GetProjectDashboardQueryValidator : AbstractValidator<GetProjectDashboardQuery>
{
    public GetProjectDashboardQueryValidator()
    {
        RuleFor(query => query.ProjectId)
            .NotEmpty();
    }
}
