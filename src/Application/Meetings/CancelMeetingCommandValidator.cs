using FluentValidation;

namespace Application.Meetings;

public sealed class CancelMeetingCommandValidator : AbstractValidator<CancelMeetingCommand>
{
    public CancelMeetingCommandValidator()
    {
        RuleFor(command => command.MeetingId)
            .NotEmpty();
    }
}
