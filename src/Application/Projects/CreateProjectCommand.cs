using Application.Projects.Dtos;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record CreateProjectCommand(
    Guid ClientId,
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    string Currency,
    IReadOnlyCollection<CreateProjectMilestoneScaffoldItem>? Milestones = null) : IRequest<Result<CreateProjectResultDto>>;

public sealed record CreateProjectMilestoneScaffoldItem(
    string Name,
    DateOnly DueDate);
