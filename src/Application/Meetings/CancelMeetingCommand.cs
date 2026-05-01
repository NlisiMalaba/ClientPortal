using MediatR;
using Shared;

namespace Application.Meetings;

public sealed record CancelMeetingCommand(Guid MeetingId) : IRequest<Result>;
