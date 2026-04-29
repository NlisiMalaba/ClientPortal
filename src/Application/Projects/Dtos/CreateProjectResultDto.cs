namespace Application.Projects.Dtos;

public sealed record CreateProjectResultDto(
    Guid ProjectId,
    IReadOnlyCollection<Guid> MilestoneIds);
