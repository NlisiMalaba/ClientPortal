using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class UpdateMilestoneCommandHandler : IRequestHandler<UpdateMilestoneCommand, Result>
{
    private static readonly Error MilestoneNotFoundError = new(
        "Milestones.NotFound",
        "Milestone was not found.",
        ErrorType.NotFound);

    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMilestoneCommandHandler(
        IMilestoneRepository milestoneRepository,
        IUnitOfWork unitOfWork)
    {
        _milestoneRepository = milestoneRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMilestoneCommand request, CancellationToken cancellationToken)
    {
        Milestone? milestone = await _milestoneRepository.FindByIdAsync(request.MilestoneId, cancellationToken);
        if (milestone is null)
        {
            return Result.Failure(MilestoneNotFoundError);
        }

        milestone.UpdateName(request.Name);
        milestone.Reschedule(request.DueDate);

        if (request.Status == MilestoneStatus.Completed)
        {
            milestone.MarkCompleted(request.CompletedAtUtc);
        }
        else
        {
            milestone.Reopen();
        }

        _milestoneRepository.Update(milestone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
