using Application.Meetings.Dtos;
using Domain;
using Shared;

namespace Application.Meetings.Abstractions;

public interface IMeetingRepository
{
    Task<Meeting?> FindByIdAsync(Guid meetingId, CancellationToken cancellationToken = default);

    Task<PagedResult<MeetingListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? clientId,
        DateTime? scheduledFrom,
        DateTime? scheduledTo,
        MeetingStatus? status,
        CancellationToken cancellationToken = default);

    void Add(Meeting meeting);

    void Update(Meeting meeting);
}
