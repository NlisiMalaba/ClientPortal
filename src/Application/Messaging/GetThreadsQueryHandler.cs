using Application.Messaging.Abstractions;
using Application.Messaging.Dtos;
using MediatR;
using Shared;

namespace Application.Messaging;

public sealed class GetThreadsQueryHandler : IRequestHandler<GetThreadsQuery, Result<PagedResult<MessageThreadListItemDto>>>
{
    private readonly IMessageThreadRepository _messageThreadRepository;

    public GetThreadsQueryHandler(IMessageThreadRepository messageThreadRepository)
    {
        _messageThreadRepository = messageThreadRepository;
    }

    public async Task<Result<PagedResult<MessageThreadListItemDto>>> Handle(
        GetThreadsQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<MessageThreadListItemDto> threads = await _messageThreadRepository.GetPagedForParticipantAsync(
            participantId: request.ParticipantId,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        return Result<PagedResult<MessageThreadListItemDto>>.Success(threads);
    }
}
