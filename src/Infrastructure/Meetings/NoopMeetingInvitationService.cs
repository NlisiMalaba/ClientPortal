using Application.Meetings.Abstractions;
using Domain;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Meetings;

public sealed class NoopMeetingInvitationService : IMeetingInvitationService
{
    private readonly ILogger<NoopMeetingInvitationService> _logger;

    public NoopMeetingInvitationService(ILogger<NoopMeetingInvitationService> logger)
    {
        _logger = logger;
    }

    public Task SendCalendarInviteAsync(Meeting meeting, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calendar invite dispatch is not configured yet for meeting {MeetingId}.",
            meeting.Id);

        return Task.CompletedTask;
    }
}
