using Application.Abstractions;
using Application.Meetings.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Meetings;

public sealed class CancelMeetingCommandHandler : IRequestHandler<CancelMeetingCommand, Result>
{
    private static readonly Error MeetingNotFoundError = new(
        "Meetings.NotFound",
        "Meeting was not found.",
        ErrorType.NotFound);

    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelMeetingCommandHandler(
        IMeetingRepository meetingRepository,
        IUnitOfWork unitOfWork)
    {
        _meetingRepository = meetingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelMeetingCommand request, CancellationToken cancellationToken)
    {
        Meeting? meeting = await _meetingRepository.FindByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            return Result.Failure(MeetingNotFoundError);
        }

        meeting.Cancel();
        _meetingRepository.Update(meeting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
