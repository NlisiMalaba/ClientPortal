using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result>
{
    private static readonly Error ProjectNotFoundError = new(
        "Projects.NotFound",
        "Project was not found.",
        ErrorType.NotFound);

    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        Project? project = await _projectRepository.FindByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Result.Failure(ProjectNotFoundError);
        }

        project.UpdateName(request.Name);
        project.UpdateDescription(request.Description);
        project.UpdateTimeline(request.StartDate, request.EndDate);
        project.UpdateBudget(request.Budget, request.Currency);

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
