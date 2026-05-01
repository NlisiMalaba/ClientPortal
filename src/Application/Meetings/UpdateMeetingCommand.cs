using MediatR;
using Shared;

namespace Application.Meetings;

public sealed record UpdateMeetingCommand(
    Guid MeetingId,
    string Title,
    string Description,
    DateTime ScheduledAt,
    int DurationMinutes,
    string MeetingUrl,
    IReadOnlyCollection<Guid> Attendees) : IRequest<Result>;
