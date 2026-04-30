using Application.Abstractions;
using Application.Meetings.Abstractions;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Meetings;

public sealed class ScheduleMeetingCommandHandler : IRequestHandler<ScheduleMeetingCommand, Result<Guid>>
{
    private static readonly Error InviteFailedError = new(
        "Meetings.CalendarInviteFailed",
        "Meeting was created, but sending calendar invite failed.",
        ErrorType.Unexpected);

    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingInvitationService _meetingInvitationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleMeetingCommandHandler> _logger;

    public ScheduleMeetingCommandHandler(
        IMeetingRepository meetingRepository,
        IMeetingInvitationService meetingInvitationService,
        IUnitOfWork unitOfWork,
        ILogger<ScheduleMeetingCommandHandler> logger)
    {
        _meetingRepository = meetingRepository;
        _meetingInvitationService = meetingInvitationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ScheduleMeetingCommand request, CancellationToken cancellationToken)
    {
        HashSet<Guid> attendees = request.Attendees
            .Where(attendeeId => attendeeId != Guid.Empty)
            .ToHashSet();

        Meeting meeting = Meeting.Create(
            id: Guid.CreateVersion7(),
            clientId: request.ClientId,
            title: request.Title,
            description: request.Description,
            scheduledAt: request.ScheduledAt,
            durationMinutes: request.DurationMinutes,
            meetingUrl: request.MeetingUrl,
            attendees: attendees);

        meeting.RaiseScheduledEvent(DateTime.UtcNow);
        _meetingRepository.Add(meeting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _meetingInvitationService.SendCalendarInviteAsync(meeting, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send calendar invite for meeting {MeetingId}.", meeting.Id);
            return Result<Guid>.Failure(InviteFailedError);
        }

        return Result<Guid>.Success(meeting.Id);
    }
}
