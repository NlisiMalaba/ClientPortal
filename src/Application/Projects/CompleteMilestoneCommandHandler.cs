using Application.Abstractions;
using Application.Projects.Abstractions;
using Application.Projects.Dtos;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class CompleteMilestoneCommandHandler : IRequestHandler<CompleteMilestoneCommand, Result<CompleteMilestoneResultDto>>
{
    private static readonly Error MilestoneNotFoundError = new(
        "Milestones.NotFound",
        "Milestone was not found.",
        ErrorType.NotFound);

    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteMilestoneCommandHandler(
        IMilestoneRepository milestoneRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _milestoneRepository = milestoneRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CompleteMilestoneResultDto>> Handle(CompleteMilestoneCommand request, CancellationToken cancellationToken)
    {
        Milestone? milestone = await _milestoneRepository.FindByIdAsync(request.MilestoneId, cancellationToken);
        if (milestone is null)
        {
            return Result<CompleteMilestoneResultDto>.Failure(MilestoneNotFoundError);
        }

        milestone.MarkCompleted(request.CompletedAtUtc ?? DateTime.UtcNow);
        _milestoneRepository.Update(milestone);

        bool hasIncompleteMilestones = await _milestoneRepository.ExistsIncompleteByProjectIdAsync(milestone.ProjectId, cancellationToken);
        bool allProjectMilestonesCompleted = !hasIncompleteMilestones;
        if (allProjectMilestonesCompleted)
        {
            Project? project = await _projectRepository.FindByIdAsync(milestone.ProjectId, cancellationToken);
            if (project is not null && project.Status != ProjectStatus.Completed)
            {
                project.UpdateStatus(ProjectStatus.Completed);
                _projectRepository.Update(project);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        CompleteMilestoneResultDto result = new(milestone.Id, allProjectMilestonesCompleted);
        return Result<CompleteMilestoneResultDto>.Success(result);
    }
}
