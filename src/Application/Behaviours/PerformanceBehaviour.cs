using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Behaviours;

public sealed class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private const long WarningThresholdMs = 500;

    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;

    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        TResponse response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > WarningThresholdMs)
        {
            _logger.LogWarning(
                "Long running request detected: {RequestName} took {ElapsedMilliseconds}ms (threshold: {WarningThresholdMs}ms).",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds,
                WarningThresholdMs);
        }

        return response;
    }
}
