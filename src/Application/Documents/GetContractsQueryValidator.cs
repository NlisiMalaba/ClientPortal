using FluentValidation;

namespace Application.Documents;

public sealed class GetContractsQueryValidator : AbstractValidator<GetContractsQuery>
{
    public GetContractsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 200);
    }
}
