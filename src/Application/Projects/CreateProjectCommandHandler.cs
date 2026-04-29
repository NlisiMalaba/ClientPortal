using Application.Abstractions;
using Application.Projects.Abstractions;
using Application.Projects.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<CreateProjectResultDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IMilestoneRepository milestoneRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _milestoneRepository = milestoneRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateProjectResultDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        Project project = Project.Create(
            id: Guid.CreateVersion7(),
            clientId: request.ClientId,
            name: request.Name,
            description: request.Description,
            status: ProjectStatus.Planned,
            startDate: request.StartDate,
            endDate: request.EndDate,
            budget: request.Budget,
            currency: request.Currency);

        _projectRepository.Add(project);

        List<Guid> milestoneIds = [];
        if (request.Milestones is not null)
        {
            foreach (CreateProjectMilestoneScaffoldItem scaffold in request.Milestones)
            {
                Milestone milestone = Milestone.Create(
                    id: Guid.CreateVersion7(),
                    projectId: project.Id,
                    name: scaffold.Name,
                    dueDate: scaffold.DueDate);

                _milestoneRepository.Add(milestone);
                milestoneIds.Add(milestone.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        CreateProjectResultDto result = new(project.Id, milestoneIds.AsReadOnly());
        return Result<CreateProjectResultDto>.Success(result);
    }
}
