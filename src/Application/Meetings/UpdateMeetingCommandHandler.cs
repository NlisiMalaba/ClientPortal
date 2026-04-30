using Application.Abstractions;
using Application.Meetings.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Meetings;

public sealed class UpdateMeetingCommandHandler : IRequestHandler<UpdateMeetingCommand, Result>
{
    private static readonly Error MeetingNotFoundError = new(
        "Meetings.NotFound",
        "Meeting was not found.",
        ErrorType.NotFound);

    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMeetingCommandHandler(
        IMeetingRepository meetingRepository,
        IUnitOfWork unitOfWork)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMeetingCommand request, CancellationToken cancellationToken)
    {
        Meeting? meeting = await _meetingRepository.FindByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            return Result.Failure(MeetingNotFoundError);
        }

        meeting.UpdateDetails(request.Title, request.Description, request.DurationMinutes, request.MeetingUrl);
        meeting.Reschedule(request.ScheduledAt);
        meeting.ReplaceAttendees(request.Attendees.Where(attendeeId => attendeeId != Guid.Empty));

        _meetingRepository.Update(meeting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
