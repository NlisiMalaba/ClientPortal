using FluentValidation;

namespace Application.Meetings;

public sealed class GetMeetingsQueryValidator : AbstractValidator<GetMeetingsQuery>
{
    public GetMeetingsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 200);

        RuleFor(query => query)
            .Must(query => !query.ScheduledFrom.HasValue || !query.ScheduledTo.HasValue || query.ScheduledFrom <= query.ScheduledTo)
            .WithMessage("ScheduledFrom must be less than or equal to ScheduledTo.");
    }
}
