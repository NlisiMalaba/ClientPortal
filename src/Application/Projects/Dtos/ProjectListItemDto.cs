using Domain;

namespace Application.Projects.Dtos;

public sealed record ProjectListItemDto(
    Guid Id,
    Guid ClientId,
    string Name,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    string Currency);
