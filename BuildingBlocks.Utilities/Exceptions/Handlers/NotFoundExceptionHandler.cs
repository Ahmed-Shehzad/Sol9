using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Utilities.Exceptions.Handlers;

public class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }
        
        logger.LogError(
            notFoundException,
            "Exception occurred: {Messages}",
            notFoundException.Message);
        
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = notFoundException.Message,
                Instance = httpContext.Request.Path
            },
            Exception = exception
        };

        return await problemDetailsService.TryWriteAsync(context);
    }
}