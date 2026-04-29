using MediatR;
using Shared;

namespace Application.Projects;

public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    string Currency) : IRequest<Result>;
