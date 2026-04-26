using FluentValidation;
using MediatR;
using Shared;

namespace Application.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IReadOnlyCollection<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators.ToList().AsReadOnly();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Count == 0)
        {
            return await next();
        }

        ValidationContext<TRequest> validationContext = new(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(validationContext, cancellationToken)));

        Error[] errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => new Error(
                string.IsNullOrWhiteSpace(failure.PropertyName) ? "Validation.Error" : $"Validation.{failure.PropertyName}",
                failure.ErrorMessage,
                ErrorType.Validation))
            .Distinct()
            .ToArray();

        if (errors.Length == 0)
        {
            return await next();
        }

        return CreateFailure(errors);
    }

    private static TResponse CreateFailure(IReadOnlyList<Error> errors)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errors);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type resultType = typeof(TResponse);
            Type valueType = resultType.GetGenericArguments()[0];
            Type genericResultType = typeof(Result<>).MakeGenericType(valueType);
            System.Reflection.MethodInfo? failureMethod = genericResultType.GetMethod(
                nameof(Result<object>.Failure),
                [typeof(IReadOnlyList<Error>)]);

            if (failureMethod is null)
            {
                throw new InvalidOperationException($"Unable to create validation failure for response type '{resultType.Name}'.");
            }

            object? failure = failureMethod.Invoke(null, [errors]);
            if (failure is TResponse typedFailure)
            {
                return typedFailure;
            }
        }

        throw new InvalidOperationException(
            $"Validation behaviour only supports responses of type Result or Result<T>. Current type: {typeof(TResponse).Name}.");
    }
}
