using MediatR;
using Shared;

namespace Application.Messaging;

public sealed record SendMessageCommand(
    Guid ThreadId,
    Guid SenderId,
    string SenderRole,
    string ClientMessageId,
    string Content) : IRequest<Result<Guid>>;
