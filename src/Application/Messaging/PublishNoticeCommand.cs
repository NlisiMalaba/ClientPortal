using MediatR;
using Shared;

namespace Application.Messaging;

public sealed record PublishNoticeCommand(
    string Title,
    string Content,
    DateTime? ExpiresAt,
    IReadOnlyCollection<Guid>? TargetClientIds) : IRequest<Result<Guid>>;
