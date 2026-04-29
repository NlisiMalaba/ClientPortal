using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, Result>
{
    private static readonly Error TaskNotFoundError = new(
        "Tasks.NotFound",
        "Task was not found.",
        ErrorType.NotFound);

    private readonly IProjectTaskRepository _projectTaskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeTaskStatusCommandHandler(
        IProjectTaskRepository projectTaskRepository,
        IUnitOfWork unitOfWork)
    {
        _projectTaskRepository = projectTaskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        ProjectTask? task = await _projectTaskRepository.FindByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return Result.Failure(TaskNotFoundError);
        }

        task.UpdateStatus(request.Status);

        _projectTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
