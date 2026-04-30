using Application.Messaging.Dtos;
using MediatR;
using Shared;

namespace Application.Messaging;

public sealed record GetThreadsQuery(
    Guid ParticipantId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<MessageThreadListItemDto>>>;
