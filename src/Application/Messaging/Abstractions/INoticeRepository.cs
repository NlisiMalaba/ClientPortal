using Application.Messaging.Dtos;
using Domain;
using Shared;

namespace Application.Messaging.Abstractions;

public interface INoticeRepository
{
    Task<PagedResult<NoticeListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? clientId,
        bool activeOnly,
        CancellationToken cancellationToken = default);

    void Add(Notice notice);
}
