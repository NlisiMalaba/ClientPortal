using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record UpdateMilestoneCommand(
    Guid MilestoneId,
    string Name,
    DateOnly DueDate,
    MilestoneStatus Status,
    DateTime? CompletedAtUtc = null) : IRequest<Result>;
