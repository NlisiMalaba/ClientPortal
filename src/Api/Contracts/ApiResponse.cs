using System.Text.Json.Serialization;

namespace Api.Contracts;

public sealed record ApiError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("type")] string? Type = null);

public sealed record ApiResponse<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("errors")] IReadOnlyList<ApiError> Errors,
    [property: JsonPropertyName("meta")] IReadOnlyDictionary<string, object?> Meta)
{
    public static ApiResponse<T> Ok(T data, IReadOnlyDictionary<string, object?>? meta = null)
    {
        return new ApiResponse<T>(true, data, [], meta ?? EmptyMeta);
    }

    public static ApiResponse<T> Fail(IReadOnlyList<ApiError> errors, IReadOnlyDictionary<string, object?>? meta = null)
    {
        return new ApiResponse<T>(false, default, errors, meta ?? EmptyMeta);
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyMeta = new Dictionary<string, object?>();
}
