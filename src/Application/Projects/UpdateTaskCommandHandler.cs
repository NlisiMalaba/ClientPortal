using Application.Abstractions;
using Application.Projects.Abstractions;
using Domain;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result>
{
    private static readonly Error TaskNotFoundError = new(
        "Tasks.NotFound",
        "Task was not found.",
        ErrorType.NotFound);

    private readonly IProjectTaskRepository _projectTaskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(
        IProjectTaskRepository projectTaskRepository,
        IUnitOfWork unitOfWork)
    {
        _projectTaskRepository = projectTaskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        ProjectTask? task = await _projectTaskRepository.FindByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return Result.Failure(TaskNotFoundError);
        }

        task.UpdateTitle(request.Title);
        task.Reassign(request.AssigneeId);
        task.UpdatePriority(request.Priority);
        task.Reschedule(request.DueDate);

        _projectTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
