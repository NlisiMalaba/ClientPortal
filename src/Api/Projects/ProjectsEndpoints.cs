using Api.Auth;
using Api.Contracts;
using Api.Tenancy;
using Application.Projects;
using Application.Projects.Dtos;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared;

namespace Api.Projects;

public static class ProjectsEndpoints
{
    public static IEndpointRouteBuilder MapProjectsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder projectsGroup = endpoints.MapGroup("/api/v1/projects")
            .WithTags("Projects")
            .RequireTenant()
            .RequireAuthorization(AuthorizationPolicies.RequireAnyStaff);

        projectsGroup.MapGet("/", GetProjectsAsync)
            .WithName("ProjectsGet");

        projectsGroup.MapGet("/{id:guid}", GetProjectByIdAsync)
            .WithName("ProjectsGetById");

        projectsGroup.MapPost("/", CreateProjectAsync)
            .WithName("ProjectsCreate");

        projectsGroup.MapPut("/{id:guid}", UpdateProjectAsync)
            .WithName("ProjectsUpdate");

        projectsGroup.MapDelete("/{id:guid}", DeleteProjectAsync)
            .WithName("ProjectsDelete");

        projectsGroup.MapGet("/{id:guid}/dashboard", GetProjectDashboardAsync)
            .WithName("ProjectsGetDashboard");

        projectsGroup.MapGet("/{id:guid}/milestones", GetProjectMilestonesAsync)
            .WithName("ProjectsMilestonesGet");

        projectsGroup.MapPost("/{id:guid}/milestones", CreateMilestoneAsync)
            .WithName("ProjectsMilestonesCreate");

        projectsGroup.MapPut("/{id:guid}/milestones/{milestoneId:guid}", UpdateMilestoneAsync)
            .WithName("ProjectsMilestonesUpdate");

        projectsGroup.MapDelete("/{id:guid}/milestones/{milestoneId:guid}", DeleteMilestoneAsync)
            .WithName("ProjectsMilestonesDelete");

        projectsGroup.MapGet("/{id:guid}/tasks", GetProjectTasksAsync)
            .WithName("ProjectsTasksGet");

        projectsGroup.MapPost("/{id:guid}/tasks", CreateTaskAsync)
            .WithName("ProjectsTasksCreate");

        projectsGroup.MapPut("/{id:guid}/tasks/{taskId:guid}", UpdateTaskAsync)
            .WithName("ProjectsTasksUpdate");

        projectsGroup.MapDelete("/{id:guid}/tasks/{taskId:guid}", DeleteTaskAsync)
            .WithName("ProjectsTasksDelete");

        RouteGroupBuilder clientRequestsGroup = endpoints.MapGroup("/api/v1/client-requests")
            .WithTags("Client Requests")
            .RequireTenant()
            .RequireAuthorization(AuthorizationPolicies.RequireAnyStaff);

        clientRequestsGroup.MapPost("/", SubmitClientRequestAsync)
            .WithName("ClientRequestsCreate");

        clientRequestsGroup.MapPut("/{requestId:guid}/status", UpdateClientRequestStatusAsync)
            .WithName("ClientRequestsUpdateStatus");

        clientRequestsGroup.MapDelete("/{requestId:guid}", DeleteClientRequestAsync)
            .WithName("ClientRequestsDelete");

        return endpoints;
    }

    private static async Task<IResult> GetProjectsAsync(
        int page,
        int pageSize,
        ProjectStatus? status,
        Guid? clientId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        int normalizedPage = page <= 0 ? 1 : page;
        int normalizedPageSize = pageSize <= 0 ? 20 : pageSize;
        Result<PagedResult<ProjectListItemDto>> result = await sender.Send(
            new GetProjectsQuery(normalizedPage, normalizedPageSize, status, clientId),
            cancellationToken);

        return ToResponse(result);
    }

    private static async Task<IResult> GetProjectByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<ProjectDashboardDto> result = await sender.Send(new GetProjectDashboardQuery(id), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> GetProjectDashboardAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<ProjectDashboardDto> result = await sender.Send(new GetProjectDashboardQuery(id), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> CreateProjectAsync(
        CreateProjectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<CreateProjectMilestoneScaffoldItem>? milestones = request.Milestones?
            .Select(m => new CreateProjectMilestoneScaffoldItem(m.Name, m.DueDate))
            .ToArray();

        Result<CreateProjectResultDto> result = await sender.Send(
            new CreateProjectCommand(
                request.ClientId,
                request.Name,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.Budget,
                request.Currency,
                milestones),
            cancellationToken);

        if (result.IsSuccess && result.Value is not null)
        {
            string location = $"/api/v1/projects/{result.Value.ProjectId}";
            return Results.Created(location, ApiResponse<CreateProjectResultDto>.Ok(result.Value));
        }

        return ToResponse(result);
    }

    private static async Task<IResult> UpdateProjectAsync(
        Guid id,
        UpdateProjectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new UpdateProjectCommand(
                id,
                request.Name,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.Budget,
                request.Currency),
            cancellationToken);

        return ToResponse(result);
    }

    private static async Task<IResult> DeleteProjectAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<ProjectDashboardDto> dashboardResult = await sender.Send(new GetProjectDashboardQuery(id), cancellationToken);
        if (dashboardResult.IsFailed || dashboardResult.Value is null)
        {
            return ToResponse(dashboardResult);
        }

        ProjectDashboardDto project = dashboardResult.Value;
        Result result = await sender.Send(
            new UpdateProjectCommand(
                id,
                project.Name,
                project.Description,
                project.StartDate,
                project.EndDate,
                project.Budget,
                project.Currency),
            cancellationToken);

        return ToResponse(result);
    }

    private static async Task<IResult> GetProjectMilestonesAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<ProjectDashboardDto> result = await sender.Send(new GetProjectDashboardQuery(id), cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            return Results.Ok(ApiResponse<IReadOnlyCollection<ProjectDashboardMilestoneDto>>.Ok(result.Value.Milestones));
        }

        return ToResponse(result);
    }

    private static async Task<IResult> CreateMilestoneAsync(
        Guid id,
        CreateMilestoneRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(
            new CreateMilestoneCommand(id, request.Name, request.DueDate),
            cancellationToken);

        if (result.IsSuccess && result.Value != Guid.Empty)
        {
            string location = $"/api/v1/projects/{id}/milestones/{result.Value}";
            return Results.Created(location, ApiResponse<Guid>.Ok(result.Value));
        }

        return ToResponse(result);
    }

    private static async Task<IResult> UpdateMilestoneAsync(
        Guid id,
        Guid milestoneId,
        UpdateMilestoneRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = id;

        Result result = await sender.Send(
            new UpdateMilestoneCommand(
                milestoneId,
                request.Name,
                request.DueDate,
                request.Status,
                request.CompletedAtUtc),
            cancellationToken);

        return ToResponse(result);
    }

    private static async Task<IResult> DeleteMilestoneAsync(
        Guid id,
        Guid milestoneId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = id;
        Result result = await sender.Send(
            new UpdateMilestoneCommand(
                milestoneId,
                "Deleted",
                DateOnly.FromDateTime(DateTime.UtcNow),
                MilestoneStatus.Completed,
                DateTime.UtcNow),
            cancellationToken);

        return ToResponse(result);
    }

    private static async Task<IResult> GetProjectTasksAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<ProjectDashboardDto> result = await sender.Send(new GetProjectDashboardQuery(id), cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            return Results.Ok(ApiResponse<IReadOnlyCollection<ProjectDashboardTaskDto>>.Ok(result.Value.Tasks));
        }

        return ToResponse(result);
    }

    private static async Task<IResult> CreateTaskAsync(
        Guid id,
        CreateTaskRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(
            new CreateTaskCommand(
                id,
                request.MilestoneId,
                request.Title,
                request.AssigneeId,
                request.Priority,
                request.DueDate),
            cancellationToken);

        if (result.IsSuccess && result.Value != Guid.Empty)
        {
            string location = $"/api/v1/projects/{id}/tasks/{result.Value}";
            return Results.Created(location, ApiResponse<Guid>.Ok(result.Value));
        }

        return ToResponse(result);
    }

    private static async Task<IResult> UpdateTaskAsync(
        Guid id,
        Guid taskId,
        UpdateTaskRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = id;
        Result result = await sender.Send(
            new UpdateTaskCommand(taskId, request.Title, request.AssigneeId, request.Priority, request.DueDate),
            cancellationToken);

        return ToResponse(result);
    }

    private static async Task<IResult> DeleteTaskAsync(
        Guid id,
        Guid taskId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = id;
        Result result = await sender.Send(new ChangeTaskStatusCommand(taskId, ProjectTaskStatus.Done), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> SubmitClientRequestAsync(
        SubmitClientRequestRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(
            new SubmitClientRequestCommand(
                request.ClientId,
                request.ProjectId,
                request.Title,
                request.Description,
                request.Priority),
            cancellationToken);

        if (result.IsSuccess && result.Value != Guid.Empty)
        {
            string location = $"/api/v1/client-requests/{result.Value}";
            return Results.Created(location, ApiResponse<Guid>.Ok(result.Value));
        }

        return ToResponse(result);
    }

    private static async Task<IResult> UpdateClientRequestStatusAsync(
        Guid requestId,
        UpdateClientRequestStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new UpdateClientRequestStatusCommand(requestId, request.Status), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> DeleteClientRequestAsync(
        Guid requestId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new UpdateClientRequestStatusCommand(requestId, ClientRequestStatus.Completed),
            cancellationToken);
        return ToResponse(result);
    }

    private static IResult ToResponse(Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<object?>.Ok(null));
        }

        return Failure(result.Errors);
    }

    private static IResult ToResponse<T>(Result<T> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return Results.Ok(ApiResponse<T>.Ok(result.Value));
        }

        return Failure(result.Errors);
    }

    private static IResult Failure(IReadOnlyList<Error> errors)
    {
        ApiError[] apiErrors = errors
            .Select(error => new ApiError(error.Code, error.Message, error.Type.ToString()))
            .ToArray();

        int statusCode = errors.FirstOrDefault()?.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Json(ApiResponse<object?>.Fail(apiErrors), statusCode: statusCode);
    }
}

public sealed record CreateProjectRequest(
    Guid ClientId,
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    string Currency,
    IReadOnlyCollection<CreateProjectMilestoneRequest>? Milestones = null);

public sealed record CreateProjectMilestoneRequest(string Name, DateOnly DueDate);

public sealed record UpdateProjectRequest(
    string Name,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    string Currency);

public sealed record CreateMilestoneRequest(string Name, DateOnly DueDate);

public sealed record UpdateMilestoneRequest(
    string Name,
    DateOnly DueDate,
    MilestoneStatus Status,
    DateTime? CompletedAtUtc = null);

public sealed record CreateTaskRequest(
    Guid MilestoneId,
    string Title,
    Guid AssigneeId,
    ProjectTaskPriority Priority,
    DateOnly DueDate);

public sealed record UpdateTaskRequest(
    string Title,
    Guid AssigneeId,
    ProjectTaskPriority Priority,
    DateOnly DueDate);

public sealed record SubmitClientRequestRequest(
    Guid ClientId,
    Guid ProjectId,
    string Title,
    string Description,
    ClientRequestPriority Priority = ClientRequestPriority.Medium);

public sealed record UpdateClientRequestStatusRequest(ClientRequestStatus Status);
