using Application.Abstractions;
using Application.Messaging.Abstractions;
using Application.Messaging.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Messaging;

public sealed class MarkThreadReadCommandHandler : IRequestHandler<MarkThreadReadCommand, Result<int>>
{
    private static readonly Error ThreadNotFoundError = new(
        "Messages.ThreadNotFound",
        "Message thread was not found.",
        ErrorType.NotFound);

    private static readonly Error ReaderNotParticipantError = new(
        "Messages.ReaderNotParticipant",
        "Reader is not a participant in this thread.",
        ErrorType.Forbidden);

    private readonly IMessageThreadRepository _messageThreadRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IRealtimeMessagingService _realtimeMessagingService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkThreadReadCommandHandler(
        IMessageThreadRepository messageThreadRepository,
        IMessageRepository messageRepository,
        IRealtimeMessagingService realtimeMessagingService,
        IUnitOfWork unitOfWork)
    {
        _messageThreadRepository = messageThreadRepository;
        _messageRepository = messageRepository;
        _realtimeMessagingService = realtimeMessagingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(MarkThreadReadCommand request, CancellationToken cancellationToken)
    {
        MessageThread? thread = await _messageThreadRepository.FindByIdAsync(request.ThreadId, cancellationToken);
        if (thread is null)
        {
            return Result<int>.Failure(ThreadNotFoundError);
        }

        if (!thread.Participants.Contains(request.ReaderId))
        {
            return Result<int>.Failure(ReaderNotParticipantError);
        }

        IReadOnlyList<Message> unreadMessages = await _messageRepository.GetUnreadMessagesForReaderAsync(
            request.ThreadId,
            request.ReaderId,
            cancellationToken);

        DateTime readAt = DateTime.UtcNow;
        foreach (Message message in unreadMessages)
        {
            message.MarkRead(readAt);
        }

        if (unreadMessages.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _realtimeMessagingService.BroadcastReadReceiptAsync(
                new RealtimeReadReceiptPayload(
                    request.ThreadId,
                    request.ReaderId,
                    unreadMessages.Count,
                    readAt),
                cancellationToken);
        }

        return Result<int>.Success(unreadMessages.Count);
    }
}
