using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    Guid AssigneeId,
    ProjectTaskPriority Priority,
    DateOnly DueDate) : IRequest<Result>;
