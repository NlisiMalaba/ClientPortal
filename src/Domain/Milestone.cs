using Shared;

namespace Domain;

public sealed class Milestone : AggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateOnly DueDate { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public MilestoneStatus Status { get; private set; } = MilestoneStatus.Planned;

    private Milestone()
    {
    }

    private Milestone(
        Guid id,
        Guid projectId,
        string name,
        DateOnly dueDate,
        DateTime? completedAt,
        MilestoneStatus status)
        : base(id)
    {
        ProjectId = NormalizeProjectId(projectId);
        Name = NormalizeName(name);
        DueDate = dueDate;
        Status = status;
        CompletedAt = NormalizeCompletedAt(completedAt);
        ValidateStatusConsistency(Status, CompletedAt);
    }

    public static Milestone Create(
        Guid id,
        Guid projectId,
        string name,
        DateOnly dueDate,
        DateTime? completedAt = null,
        MilestoneStatus status = MilestoneStatus.Planned)
    {
        return new Milestone(id, projectId, name, dueDate, completedAt, status);
    }

    public void UpdateName(string name)
    {
        Name = NormalizeName(name);
        MarkUpdated();
    }

    public void Reschedule(DateOnly dueDate)
    {
        DueDate = dueDate;
        MarkUpdated();
    }

    public void MarkCompleted(DateTime? completedAtUtc = null)
    {
        Status = MilestoneStatus.Completed;
        DateTime completedAt = NormalizeCompletedAt(completedAtUtc ?? DateTime.UtcNow)
            ?? throw new InvalidOperationException("CompletedAt cannot be null when marking milestone as completed.");
        CompletedAt = completedAt;
        AddDomainEvent(new MilestoneCompletedEvent(Id, ProjectId, completedAt));
        MarkUpdated();
    }

    public void Reopen()
    {
        Status = MilestoneStatus.Planned;
        CompletedAt = null;
        MarkUpdated();
    }

    private static Guid NormalizeProjectId(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("ProjectId cannot be empty.", nameof(projectId));
        }

        return projectId;
    }

    private static string NormalizeName(string name)
    {
        return Guard.NotEmpty(name, nameof(name)).Trim();
    }

    private static DateTime? NormalizeCompletedAt(DateTime? completedAtUtc)
    {
        if (!completedAtUtc.HasValue)
        {
            return null;
        }

        DateTime normalized = completedAtUtc.Value;
        if (normalized.Kind == DateTimeKind.Local)
        {
            normalized = normalized.ToUniversalTime();
        }
        else if (normalized.Kind == DateTimeKind.Unspecified)
        {
            normalized = DateTime.SpecifyKind(normalized, DateTimeKind.Utc);
        }

        return normalized;
    }

    private static void ValidateStatusConsistency(MilestoneStatus status, DateTime? completedAt)
    {
        if (status == MilestoneStatus.Completed && !completedAt.HasValue)
        {
            throw new ArgumentException("Completed milestones must include CompletedAt.", nameof(completedAt));
        }

        if (status != MilestoneStatus.Completed && completedAt.HasValue)
        {
            throw new ArgumentException("Only completed milestones can include CompletedAt.", nameof(completedAt));
        }
    }
}
