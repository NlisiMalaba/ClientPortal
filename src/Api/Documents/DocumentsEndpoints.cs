using Api.Auth;
using Api.Contracts;
using Api.Tenancy;
using Application.Documents;
using Application.Documents.Dtos;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared;

namespace Api.Documents;

public static class DocumentsEndpoints
{
    public static IEndpointRouteBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder documentsGroup = endpoints.MapGroup("/api/v1/documents")
            .WithTags("Documents")
            .RequireTenant()
            .RequireAuthorization(AuthorizationPolicies.RequireAnyStaff);

        documentsGroup.MapPost("/upload-url", GetUploadUrlAsync).WithName("DocumentsGetUploadUrl");
        documentsGroup.MapPost("/confirm-upload", ConfirmUploadAsync).WithName("DocumentsConfirmUpload");
        documentsGroup.MapGet("/", GetDocumentsAsync).WithName("DocumentsGet");
        documentsGroup.MapGet("/{id:guid}/download", GetDownloadUrlAsync).WithName("DocumentsGetDownloadUrl");
        documentsGroup.MapPost("/{id:guid}/versions", UploadNewVersionAsync).WithName("DocumentsUploadVersion");

        RouteGroupBuilder contractsGroup = endpoints.MapGroup("/api/v1/contracts")
            .WithTags("Contracts")
            .RequireTenant()
            .RequireAuthorization(AuthorizationPolicies.RequireAnyStaff);

        contractsGroup.MapGet("/", GetContractsAsync).WithName("ContractsGet");
        contractsGroup.MapGet("/{id:guid}", GetContractByIdAsync).WithName("ContractsGetById");
        contractsGroup.MapPost("/", CreateContractAsync).WithName("ContractsCreate");
        contractsGroup.MapPut("/{id:guid}", UpdateContractAsync).WithName("ContractsUpdate");
        contractsGroup.MapDelete("/{id:guid}", DeleteContractAsync).WithName("ContractsDelete");
        contractsGroup.MapPost("/{id:guid}/send", SendContractForSigningAsync).WithName("ContractsSend");
        contractsGroup.MapPost("/{id:guid}/sign", RecordSignatureAsync).WithName("ContractsSign");

        return endpoints;
    }

    private static async Task<IResult> GetUploadUrlAsync(GetUploadPresignedUrlRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Result<GetUploadPresignedUrlResultDto> result = await sender.Send(
            new GetUploadPresignedUrlCommand(request.ClientId, request.ProjectId, request.Name, request.Type, request.Tags, request.UploadedBy),
            cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> ConfirmUploadAsync(ConfirmUploadRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new ConfirmUploadCommand(request.DocumentId, request.ClientId), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> GetDocumentsAsync(
        int page,
        int pageSize,
        string? type,
        Guid? projectId,
        Guid? clientId,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<DocumentListItemDto>> result = await sender.Send(
            new GetDocumentsQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                type,
                projectId,
                clientId,
                createdFromUtc,
                createdToUtc),
            cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> GetDownloadUrlAsync(Guid id, Guid clientId, ISender sender, CancellationToken cancellationToken)
    {
        Result<GetDocumentDownloadUrlResultDto> result = await sender.Send(
            new GetDocumentDownloadUrlQuery(id, clientId),
            cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> UploadNewVersionAsync(Guid id, UploadDocumentVersionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Result<int> result = await sender.Send(
            new UploadNewVersionCommand(id, request.ClientId, request.S3Key, request.UploadedBy, request.ChangeNotes, request.UploadedAtUtc),
            cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> GetContractsAsync(int page, int pageSize, Guid? clientId, ContractStatus? status, ISender sender, CancellationToken cancellationToken)
    {
        Result<PagedResult<ContractListItemDto>> result = await sender.Send(
            new GetContractsQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, clientId, status),
            cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> GetContractByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        Result<ContractDto> result = await sender.Send(new GetContractByIdQuery(id), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> CreateContractAsync(CreateContractRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Result<ContractDto> result = await sender.Send(
            new CreateContractCommand(request.ClientId, request.Title, request.S3Key, request.Parties, request.ExpiresAtUtc),
            cancellationToken);

        if (result.IsSuccess && result.Value is not null)
        {
            string location = $"/api/v1/contracts/{result.Value.Id}";
            return Results.Created(location, ApiResponse<ContractDto>.Ok(result.Value));
        }

        return ToResponse(result);
    }

    private static async Task<IResult> UpdateContractAsync(Guid id, UpdateContractRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new UpdateContractCommand(id, request.ClientId, request.Title, request.S3Key, request.Parties, request.ExpiresAtUtc),
            cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> DeleteContractAsync(Guid id, Guid clientId, ISender sender, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new DeleteContractCommand(id, clientId), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> SendContractForSigningAsync(Guid id, SendContractRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Result<SendContractForSigningResultDto> result = await sender.Send(new SendContractForSigningCommand(id, request.ClientId), cancellationToken);
        return ToResponse(result);
    }

    private static async Task<IResult> RecordSignatureAsync(Guid id, RecordSignatureRequest request, ISender sender, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new RecordSignatureCommand(id, request.ClientId, request.SignedAtUtc), cancellationToken);
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

public sealed record GetUploadPresignedUrlRequest(
    Guid ClientId,
    Guid? ProjectId,
    string Name,
    string Type,
    IReadOnlyCollection<string>? Tags,
    Guid UploadedBy);

public sealed record ConfirmUploadRequest(Guid DocumentId, Guid ClientId);

public sealed record UploadDocumentVersionRequest(
    Guid ClientId,
    string S3Key,
    Guid UploadedBy,
    string? ChangeNotes = null,
    DateTime? UploadedAtUtc = null);

public sealed record CreateContractRequest(
    Guid ClientId,
    string Title,
    string S3Key,
    IReadOnlyCollection<string>? Parties,
    DateTime? ExpiresAtUtc);

public sealed record UpdateContractRequest(
    Guid ClientId,
    string Title,
    string S3Key,
    IReadOnlyCollection<string>? Parties,
    DateTime? ExpiresAtUtc);

public sealed record SendContractRequest(Guid ClientId);

public sealed record RecordSignatureRequest(Guid ClientId, DateTime? SignedAtUtc = null);
