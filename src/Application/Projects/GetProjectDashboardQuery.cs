using Application.Projects.Dtos;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record GetProjectDashboardQuery(Guid ProjectId) : IRequest<Result<ProjectDashboardDto>>;
