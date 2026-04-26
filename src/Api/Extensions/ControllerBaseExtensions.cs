using Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Api.Extensions;

public static class ControllerBaseExtensions
{
    public static ActionResult<ApiResponse<T>> OkResult<T>(
        this ControllerBase controller,
        T data,
        IReadOnlyDictionary<string, object?>? meta = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return controller.Ok(ApiResponse<T>.Ok(data, meta));
    }

    public static ActionResult<ApiResponse<T>> CreatedResult<T>(
        this ControllerBase controller,
        string location,
        T data,
        IReadOnlyDictionary<string, object?>? meta = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("Location must be provided.", nameof(location));
        }

        return controller.Created(location, ApiResponse<T>.Ok(data, meta));
    }

    public static ActionResult<ApiResponse<object>> NotFoundResult(
        this ControllerBase controller,
        string message = "Resource not found.",
        string code = "NotFound")
    {
        ArgumentNullException.ThrowIfNull(controller);

        ApiError error = new(code, message, ErrorType.NotFound.ToString());
        return controller.NotFound(ApiResponse<object>.Fail([error]));
    }

    public static ActionResult<ApiResponse<object>> ValidationResult(
        this ControllerBase controller,
        IReadOnlyList<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("At least one validation error must be provided.", nameof(errors));
        }

        ApiError[] apiErrors = errors
            .Select(error => new ApiError(error.Code, error.Message, error.Type.ToString()))
            .ToArray();

        return controller.BadRequest(ApiResponse<object>.Fail(apiErrors));
    }
}
