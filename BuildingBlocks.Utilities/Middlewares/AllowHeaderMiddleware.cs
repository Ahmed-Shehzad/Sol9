using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Utilities.Middlewares;

/// <summary>
/// Middleware to set the Allow header in the HTTP response based on the request method.
/// </summary>
public class AllowHeaderMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
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