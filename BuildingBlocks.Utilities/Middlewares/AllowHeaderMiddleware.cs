using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Utilities.Middlewares;

/// <summary>
/// Middleware to set the Allow header in the HTTP response based on the request method.
/// </summary>
/// <param name="next">The next middleware in the pipeline.</param>
public class AllowHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var allowedMethods = context.Request.Method switch
        {
            "GET" => "GET",
            "POST" => "POST",
            "PUT" => "PUT",
            "DELETE" => "DELETE",
            _ => "GET, POST, PUT, DELETE"
        };

        context.Response.Headers.Allow = allowedMethods;
        await next(context);
    }
}