using MediatR;
using Shared;

namespace Application.Messaging;

public sealed record MarkThreadReadCommand(
    Guid ThreadId,
    Guid ReaderId) : IRequest<Result<int>>;
