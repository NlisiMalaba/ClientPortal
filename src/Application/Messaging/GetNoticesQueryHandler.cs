using Application.Messaging.Abstractions;
using Application.Messaging.Dtos;
using MediatR;
using Shared;

namespace Application.Messaging;

public sealed class GetNoticesQueryHandler : IRequestHandler<GetNoticesQuery, Result<PagedResult<NoticeListItemDto>>>
{
    private readonly INoticeRepository _noticeRepository;

    public GetNoticesQueryHandler(INoticeRepository noticeRepository)
    {
        _noticeRepository = noticeRepository;
    }

    public async Task<Result<PagedResult<NoticeListItemDto>>> Handle(
        GetNoticesQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<NoticeListItemDto> notices = await _noticeRepository.GetPagedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            clientId: request.ClientId,
            activeOnly: request.ActiveOnly,
            cancellationToken: cancellationToken);

        return Result<PagedResult<NoticeListItemDto>>.Success(notices);
    }
}
