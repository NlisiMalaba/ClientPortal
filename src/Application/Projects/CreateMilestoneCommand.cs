using MediatR;
using Shared;

namespace Application.Projects;

public sealed record CreateMilestoneCommand(
    Guid ProjectId,
    string Name,
    DateOnly DueDate) : IRequest<Result<Guid>>;
