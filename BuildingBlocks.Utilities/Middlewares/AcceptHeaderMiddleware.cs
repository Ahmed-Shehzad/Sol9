using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Utilities.Middlewares;

/// <summary>
/// Middleware to validate the Accept header in the HTTP request.
/// </summary>
/// <param name="next">The next middleware in the pipeline.</param>
public class AcceptHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Request.Headers.Accept))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid Accept header. Expected 'application/json'.");
            return;
        }
        await next(context);
    }
}