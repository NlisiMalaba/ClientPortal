using Shared;

namespace Domain;

public sealed class ProjectTask : AggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }

    public Guid MilestoneId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public Guid AssigneeId { get; private set; }

    public ProjectTaskStatus Status { get; private set; } = ProjectTaskStatus.Todo;

    public ProjectTaskPriority Priority { get; private set; } = ProjectTaskPriority.Medium;

    public DateOnly DueDate { get; private set; }

    private ProjectTask()
    {
    }

    private ProjectTask(
        Guid id,
        Guid projectId,
        Guid milestoneId,
        string title,
        Guid assigneeId,
        ProjectTaskStatus status,
        ProjectTaskPriority priority,
        DateOnly dueDate)
        : base(id)
    {
        ProjectId = NormalizeForeignId(projectId, nameof(projectId), "ProjectId");
        MilestoneId = NormalizeForeignId(milestoneId, nameof(milestoneId), "MilestoneId");
        Title = NormalizeTitle(title);
        AssigneeId = NormalizeForeignId(assigneeId, nameof(assigneeId), "AssigneeId");
        Status = status;
        Priority = priority;
        DueDate = dueDate;
    }

    public static ProjectTask Create(
        Guid id,
        Guid projectId,
        Guid milestoneId,
        string title,
        Guid assigneeId,
        ProjectTaskStatus status,
        ProjectTaskPriority priority,
        DateOnly dueDate)
    {
        return new ProjectTask(id, projectId, milestoneId, title, assigneeId, status, priority, dueDate);
    }

    public void UpdateTitle(string title)
    {
        Title = NormalizeTitle(title);
        MarkUpdated();
    }

    public void Reassign(Guid assigneeId)
    {
        AssigneeId = NormalizeForeignId(assigneeId, nameof(assigneeId), "AssigneeId");
        MarkUpdated();
    }

    public void UpdateStatus(ProjectTaskStatus status)
    {
        ProjectTaskStatus previousStatus = Status;
        Status = status;
        if (previousStatus != status)
        {
            AddDomainEvent(new TaskStatusChangedEvent(Id, ProjectId, previousStatus, status, DateTime.UtcNow));
        }

        MarkUpdated();
    }

    public void UpdatePriority(ProjectTaskPriority priority)
    {
        Priority = priority;
        MarkUpdated();
    }

    public void Reschedule(DateOnly dueDate)
    {
        DueDate = dueDate;
        MarkUpdated();
    }

    private static Guid NormalizeForeignId(Guid value, string paramName, string propertyName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{propertyName} cannot be empty.", paramName);
        }

        return value;
    }

    private static string NormalizeTitle(string title)
    {
        return Guard.NotEmpty(title, nameof(title)).Trim();
    }
}
