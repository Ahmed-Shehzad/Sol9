using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Utilities.Exceptions.Handlers;

public class BadRequestExceptionHandler (ILogger<BadRequestExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        
        if (exception is not BadRequestException badRequestException)
        {
            return false;
        }
        
        logger.LogError(
            badRequestException,
            "Exception occurred: {Messages}",
            badRequestException.Message);
        
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = badRequestException.Message,
                Instance = httpContext.Request.Path
            },
            Exception = exception
        };

        return await problemDetailsService.TryWriteAsync(context);
    }
}