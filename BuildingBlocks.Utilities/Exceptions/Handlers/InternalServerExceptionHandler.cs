using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Utilities.Exceptions.Handlers;

public class InternalServerExceptionHandler (ILogger<InternalServerExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        
        if (exception is not InternalServerException internalServerException)
        {
            return false;
        }
        
        logger.LogError(
            internalServerException,
            "Exception occurred: {Messages}",
            internalServerException.Message);
        
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = internalServerException.Message,
                Instance = httpContext.Request.Path
            },
            Exception = exception
        };

        return await problemDetailsService.TryWriteAsync(context);
    }
}