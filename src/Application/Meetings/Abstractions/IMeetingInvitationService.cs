using Domain;

namespace Application.Meetings.Abstractions;

public interface IMeetingInvitationService
{
    Task SendCalendarInviteAsync(Meeting meeting, CancellationToken cancellationToken = default);
}
