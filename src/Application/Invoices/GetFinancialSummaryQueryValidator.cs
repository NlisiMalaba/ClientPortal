using FluentValidation;

namespace Application.Invoices;

public sealed class GetFinancialSummaryQueryValidator : AbstractValidator<GetFinancialSummaryQuery>
{
    public GetFinancialSummaryQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => !query.FromDate.HasValue || !query.ToDate.HasValue || query.FromDate <= query.ToDate)
            .WithMessage("FromDate must be less than or equal to ToDate.");
    }
}
