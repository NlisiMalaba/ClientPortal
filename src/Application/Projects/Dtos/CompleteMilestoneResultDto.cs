namespace Application.Projects.Dtos;

public sealed record CompleteMilestoneResultDto(
    Guid MilestoneId,
    bool AllProjectMilestonesCompleted);
