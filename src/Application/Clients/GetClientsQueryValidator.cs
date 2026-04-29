using FluentValidation;

namespace Application.Clients;

public sealed class GetClientsQueryValidator : AbstractValidator<GetClientsQuery>
{
    public GetClientsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 200);

        RuleFor(query => query.Search)
            .MaximumLength(256);
    }
}
