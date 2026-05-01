using MediatR;
using Shared;

namespace Application.Messaging;

public sealed record CreateThreadCommand(
    Guid ClientId,
    Guid? ProjectId,
    Guid CreatorId,
    IReadOnlyCollection<Guid> ParticipantIds,
    string Subject) : IRequest<Result<Guid>>;
