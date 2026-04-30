using FluentValidation;

namespace Application.Messaging;

public sealed class GetThreadMessagesQueryValidator : AbstractValidator<GetThreadMessagesQuery>
{
    public GetThreadMessagesQueryValidator()
    {
        RuleFor(query => query.ThreadId)
            .NotEmpty();

        RuleFor(query => query.ParticipantId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 200);
    }
}
