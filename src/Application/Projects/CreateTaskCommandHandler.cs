using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    private static readonly Error ProjectNotFoundError = new(
        "Projects.NotFound",
        "Project was not found.",
        ErrorType.NotFound);

    private static readonly Error MilestoneNotFoundError = new(
        "Milestones.NotFound",
        "Milestone was not found.",
        ErrorType.NotFound);

    private readonly IProjectRepository _projectRepository;
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IProjectTaskRepository _projectTaskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(
        IProjectRepository projectRepository,
        IMilestoneRepository milestoneRepository,
        IProjectTaskRepository projectTaskRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _milestoneRepository = milestoneRepository;
        _projectTaskRepository = projectTaskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        Project? project = await _projectRepository.FindByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Result<Guid>.Failure(ProjectNotFoundError);
        }

        Milestone? milestone = await _milestoneRepository.FindByIdAsync(request.MilestoneId, cancellationToken);
        if (milestone is null || milestone.ProjectId != request.ProjectId)
        {
            return Result<Guid>.Failure(MilestoneNotFoundError);
        }

        ProjectTask task = ProjectTask.Create(
            id: Guid.CreateVersion7(),
            projectId: request.ProjectId,
            milestoneId: request.MilestoneId,
            title: request.Title,
            assigneeId: request.AssigneeId,
            status: ProjectTaskStatus.Todo,
            priority: request.Priority,
            dueDate: request.DueDate);

        _projectTaskRepository.Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(task.Id);
    }
}
