using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Behaviours;

public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        Stopwatch stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling request {RequestName}", requestName);

        try
        {
            TResponse response = await next();
            stopwatch.Stop();

            if (response.IsFailed)
            {
                _logger.LogWarning(
                    "Request {RequestName} failed in {ElapsedMilliseconds}ms with errors: {@Errors}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    response.Errors);

                return response;
            }

            _logger.LogInformation(
                "Request {RequestName} completed successfully in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _logger.LogError(
                exception,
                "Request {RequestName} threw an exception after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
