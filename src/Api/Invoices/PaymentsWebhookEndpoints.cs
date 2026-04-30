using Api.Contracts;
using Application.Invoices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared;

namespace Api.Invoices;

public static class PaymentsWebhookEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsWebhookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder webhooksGroup = endpoints.MapGroup("/api/v1/webhooks/payments")
            .WithTags("Payment Webhooks");

        webhooksGroup.MapPost("/", ProcessPaymentWebhookAsync)
            .WithName("PaymentsWebhookProcess")
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> ProcessPaymentWebhookAsync(
        ProcessPaymentWebhookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new ProcessGatewayPaymentCommand(
                request.Provider,
                request.Payload,
                request.Signature),
            cancellationToken);

        return ToResponse(result);
    }

    private static IResult ToResponse(Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<object?>.Ok(null));
        }

        ApiError[] errors = result.Errors
            .Select(error => new ApiError(error.Code, error.Message, error.Type.ToString()))
            .ToArray();

        int statusCode = result.Errors.FirstOrDefault()?.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Json(ApiResponse<object?>.Fail(errors), statusCode: statusCode);
    }
}

public sealed record ProcessPaymentWebhookRequest(
    string Provider,
    string Payload,
    string Signature);
