namespace Application.Clients.Dtos;

public sealed record ClientProjectsSummaryDto(
    int TotalProjects,
    int ActiveProjects,
    int CompletedProjects);
