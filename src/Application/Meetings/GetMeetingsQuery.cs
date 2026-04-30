using Application.Meetings.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Meetings;

public sealed record GetMeetingsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? ClientId = null,
    DateTime? ScheduledFrom = null,
    DateTime? ScheduledTo = null,
    MeetingStatus? Status = null) : IRequest<Result<PagedResult<MeetingListItemDto>>>;
