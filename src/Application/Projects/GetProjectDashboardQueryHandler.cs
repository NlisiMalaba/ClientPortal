using Application.Projects.Abstractions;
using Application.Projects.Dtos;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class GetProjectDashboardQueryHandler : IRequestHandler<GetProjectDashboardQuery, Result<ProjectDashboardDto>>
{
    private static readonly Error ProjectNotFoundError = new(
        "Projects.NotFound",
        "Project was not found.",
        ErrorType.NotFound);

    private readonly IProjectRepository _projectRepository;

    public GetProjectDashboardQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDashboardDto>> Handle(GetProjectDashboardQuery request, CancellationToken cancellationToken)
    {
        ProjectDashboardDto? dashboard = await _projectRepository.GetDashboardAsync(request.ProjectId, cancellationToken);
        if (dashboard is null)
        {
            return Result<ProjectDashboardDto>.Failure(ProjectNotFoundError);
        }

        return Result<ProjectDashboardDto>.Success(dashboard);
    }
}
