using FluentValidation;

namespace Application.Messaging;

public sealed class GetThreadsQueryValidator : AbstractValidator<GetThreadsQuery>
{
    public GetThreadsQueryValidator()
    {
        RuleFor(query => query.ParticipantId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 200);
    }
}
