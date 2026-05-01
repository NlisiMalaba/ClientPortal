using Application.Messaging.Dtos;
using MediatR;
using Shared;

namespace Application.Messaging;

public sealed record GetNoticesQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? ClientId = null,
    bool ActiveOnly = true) : IRequest<Result<PagedResult<NoticeListItemDto>>>;
