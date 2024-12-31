using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Utilities.Middlewares;

/// <summary>
/// Middleware to add security headers to the HTTP response.
/// </summary>
public class SecurityHeadersMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers.ContentSecurityPolicy = new StringValues("default-src 'self'");
        context.Response.Headers.XContentTypeOptions = new StringValues("nosniff");
        context.Response.Headers.XFrameOptions = new StringValues("SAMEORIGIN");
        context.Response.Headers.XXSSProtection = new StringValues("1; mode=block");
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // context.Response.Headers["Permissions-Policy"] = "camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), usb=()";

        await next(context);
    }
}