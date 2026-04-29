using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record SubmitClientRequestCommand(
    Guid ClientId,
    Guid ProjectId,
    string Title,
    string Description,
    ClientRequestPriority Priority = ClientRequestPriority.Medium) : IRequest<Result<Guid>>;
