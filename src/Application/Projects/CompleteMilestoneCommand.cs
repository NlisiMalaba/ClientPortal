using Application.Projects.Dtos;
using MediatR;
using Shared;

namespace Application.Projects;

public sealed record CompleteMilestoneCommand(
    Guid MilestoneId,
    DateTime? CompletedAtUtc = null) : IRequest<Result<CompleteMilestoneResultDto>>;
