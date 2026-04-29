using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record CreateTaskCommand(
    Guid ProjectId,
    Guid MilestoneId,
    string Title,
    Guid AssigneeId,
    ProjectTaskPriority Priority,
    DateOnly DueDate) : IRequest<Result<Guid>>;
