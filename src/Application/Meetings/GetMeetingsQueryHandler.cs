using Application.Meetings.Abstractions;
using Application.Meetings.Dtos;
using MediatR;
using Shared;

namespace Application.Meetings;

public sealed class GetMeetingsQueryHandler : IRequestHandler<GetMeetingsQuery, Result<PagedResult<MeetingListItemDto>>>
{
    private readonly IMeetingRepository _meetingRepository;

    public GetMeetingsQueryHandler(IMeetingRepository meetingRepository)
    {
        _meetingRepository = meetingRepository;
    }

    public async Task<Result<PagedResult<MeetingListItemDto>>> Handle(
        GetMeetingsQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<MeetingListItemDto> meetings = await _meetingRepository.GetPagedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            clientId: request.ClientId,
            scheduledFrom: request.ScheduledFrom,
            scheduledTo: request.ScheduledTo,
            status: request.Status,
            cancellationToken: cancellationToken);

        return Result<PagedResult<MeetingListItemDto>>.Success(meetings);
    }
}
