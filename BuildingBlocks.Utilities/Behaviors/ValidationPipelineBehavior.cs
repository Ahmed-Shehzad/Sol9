using System.Text.Json;
using BuildingBlocks.Utilities.Exceptions;
using BuildingBlocks.Utilities.Types;
using FluentResults;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Utilities.Behaviors;

/// <summary>
/// A behavior for validating incoming requests in a MediatR pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class ValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class 
    where TResponse : Result
{
    /// <summary>
    /// Validates incoming requests using the provided validators and throws a <see cref="BadRequestException"/> if validation fails.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response if validation passes, otherwise throws a <see cref="BadRequestException"/>.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (failures.Count == 0) 
            return await next();

        var type = typeof(TRequest).Name;

        var serializer = JsonSerializer.Serialize(failures.Select(failure => new
        {
            failure.ErrorMessage,
            failure.PropertyName,
            failure.AttemptedValue,
            failure.ErrorCode
        }).ToList(), JsonConfigurations.GetDefaultOptions());
        
        throw new BadRequestException($"Model validation failed Type {type} Errors @ {serializer}", new Error(type));
    }
}