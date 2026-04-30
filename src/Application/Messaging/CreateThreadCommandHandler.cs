using Application.Abstractions;
using Application.Messaging.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Messaging;

public sealed class CreateThreadCommandHandler : IRequestHandler<CreateThreadCommand, Result<Guid>>
{
    private readonly IMessageThreadRepository _messageThreadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateThreadCommandHandler(
        IMessageThreadRepository messageThreadRepository,
        IUnitOfWork unitOfWork)
    {
        _messageThreadRepository = messageThreadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateThreadCommand request, CancellationToken cancellationToken)
    {
        HashSet<Guid> participants = request.ParticipantIds
            .Where(participantId => participantId != Guid.Empty)
            .ToHashSet();

        participants.Add(request.CreatorId);

        DateTime createdAt = DateTime.UtcNow;
        MessageThread thread = MessageThread.Create(
            id: Guid.CreateVersion7(),
            clientId: request.ClientId,
            projectId: request.ProjectId,
            participants: participants,
            subject: request.Subject,
            lastMessageAt: createdAt);

        _messageThreadRepository.Add(thread);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(thread.Id);
    }
}
