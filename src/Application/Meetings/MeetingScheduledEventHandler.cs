using Application.Meetings.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Meetings;

public sealed class MeetingScheduledEventHandler : INotificationHandler<MeetingScheduledEvent>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingInvitationService _meetingInvitationService;
    private readonly ILogger<MeetingScheduledEventHandler> _logger;

    public MeetingScheduledEventHandler(
        IMeetingRepository meetingRepository,
        IMeetingInvitationService meetingInvitationService,
        ILogger<MeetingScheduledEventHandler> logger)
    {
        _meetingRepository = meetingRepository;
        _meetingInvitationService = meetingInvitationService;
        _logger = logger;
    }

    public async Task Handle(MeetingScheduledEvent notification, CancellationToken cancellationToken)
    {
        Meeting? meeting = await _meetingRepository.FindByIdAsync(notification.MeetingId, cancellationToken);
        if (meeting is null || meeting.ClientId != notification.ClientId)
        {
            _logger.LogWarning(
                "MeetingScheduledEvent ignored because meeting {MeetingId} was not found for client {ClientId}.",
                notification.MeetingId,
                notification.ClientId);
            return;
        }

        try
        {
            await _meetingInvitationService.SendCalendarInviteAsync(meeting, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send calendar invite for meeting {MeetingId}.", meeting.Id);
        }
    }
}
