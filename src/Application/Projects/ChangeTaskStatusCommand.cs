using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record ChangeTaskStatusCommand(
    Guid TaskId,
    ProjectTaskStatus Status) : IRequest<Result>;
