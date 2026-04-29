using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class CreateMilestoneCommandHandler : IRequestHandler<CreateMilestoneCommand, Result<Guid>>
{
    private static readonly Error ProjectNotFoundError = new(
        "Projects.NotFound",
        "Project was not found.",
        ErrorType.NotFound);

    private readonly IProjectRepository _projectRepository;
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMilestoneCommandHandler(
        IProjectRepository projectRepository,
        IMilestoneRepository milestoneRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _milestoneRepository = milestoneRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        Project? project = await _projectRepository.FindByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return Result<Guid>.Failure(ProjectNotFoundError);
        }

        Milestone milestone = Milestone.Create(
            id: Guid.CreateVersion7(),
            projectId: request.ProjectId,
            name: request.Name,
            dueDate: request.DueDate);

        _milestoneRepository.Add(milestone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(milestone.Id);
    }
}
