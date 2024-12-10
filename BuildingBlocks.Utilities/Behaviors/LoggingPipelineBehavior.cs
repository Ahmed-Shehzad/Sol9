using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BuildingBlocks.Utilities.Behaviors;

/// <summary>
/// A behavior for logging incoming requests and responses in a MediatR pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response, which must be a FluentResults.Result.</typeparam>
public class LoggingPipelineBehavior<TRequest, TResponse>(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : class 
    where TResponse : Result
{

    /// <summary>
    /// Handles the request by logging incoming requests, processing the request, and logging the response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The delegate to call to handle the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Processing Incoming Request @ {Date}: {RequestName}", DateTime.UtcNow, requestName);
        
        try
        {
            var response = await next();
            if (response.IsSuccess)
                logger.LogInformation("Processing Request @ {Date} {RequestName} was successful", DateTime.UtcNow, requestName);
            
            else
                using (LogContext.PushProperty("Errors", response.Errors, true)) 
                    logger.LogError("Processing Request @ {Date} {RequestName} failed with errors", DateTime.UtcNow, requestName);
            
            return response;
        }
        catch (Exception e)
        {
            logger.LogError("Exception Occured @ {Date} with message {Messages}", DateTime.UtcNow, e);
            throw;
        }
    }
}