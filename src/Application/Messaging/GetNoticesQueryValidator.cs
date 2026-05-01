using FluentValidation;

namespace Application.Messaging;

public sealed class GetNoticesQueryValidator : AbstractValidator<GetNoticesQuery>
{
    public GetNoticesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 200);
    }
}
