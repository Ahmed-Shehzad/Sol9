using System.Net;
using System.Threading.RateLimiting;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.Utilities.Exceptions.Handlers;
using BuildingBlocks.Utilities.Types;
using MediatR.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;

namespace BuildingBlocks.ServiceCollection.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a singleton instance of <see cref="IIdentityAccessor"/> to the service collection.
    /// </summary>
    /// <typeparam name="T">The type of identity.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIdentityProvider<T>(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(IIdentityAccessor<T>), typeof(IdentityAccessor<T>));
        services.TryAddSingleton(typeof(IIdentityAccessor), sp =>
        {
            var type = sp.GetService(typeof(IIdentityAccessor<T>));
            return (type as IIdentityAccessor)!;
        });
        return services;
    }

    /// <summary>
    /// Configures the default behavior when a user is not authenticated.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection ConfigureDefaultUnauthorizedBehavior(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
        });
        return services;
    }

    /// <summary>
    /// A class to hold configuration options for the MapDevTools method.
    /// </summary>
    public class DevToolsOptions(IDictionary<string, Func<IServiceProvider, object>> container)
    {
        /// <summary>
        /// Adds an entry to the dev tools container.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        /// <param name="value">The value for the entry.</param>
        /// <returns>The current instance of <see cref="DevToolsOptions"/>.</returns>
        public DevToolsOptions AddEntry(string key, Func<IServiceProvider, object> value)
        {
            container.TryAdd(key, value);
            return this;
        }
    }

    /// <summary>
    /// Maps a custom endpoint for serving developer tools.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The URL pattern for the endpoint.</param>
    /// <param name="configure">An action to configure the <see cref="DevToolsOptions"/>.</param>
    /// <returns>The current instance of <see cref="IEndpointRouteBuilder"/>.</returns>
    public static IEndpointRouteBuilder MapDevTools(
        this IEndpointRouteBuilder endpoints,
        string pattern, Action<DevToolsOptions> configure)
    {
        endpoints.Map(pattern, ctx =>
        {
            var container = new Dictionary<string, Func<IServiceProvider, object>>();
            configure(new DevToolsOptions(container));
            var compiled = container.ToDictionary(t => t.Key,
                t => t.Value.Invoke(ctx.RequestServices));
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            return ctx.Response.WriteAsync(JsonConvert.SerializeObject(compiled));
        });
        return endpoints;
    }

    /// <summary>
    /// Maps a redirect endpoint.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="fromPattern">The URL pattern to redirect from.</param>
    /// <param name="toPattern">The URL pattern to redirect to.</param>
    /// <param name="pathBase">The optional path base for the redirect.</param>
    /// <returns>The current instance of <see cref="IEndpointRouteBuilder"/>.</returns>
    public static IEndpointRouteBuilder MapRedirect(
        this IEndpointRouteBuilder endpoints,
        PathString fromPattern, PathString toPattern, string? pathBase = null)
    {
        if (!pathBase.IsNullOrWhiteSpace())
        {
            PathString path = pathBase!.TrimEnd('/');
            if (!toPattern.StartsWithSegments(pathBase))
            {
                toPattern = path.Add(toPattern);
            }
        }
        endpoints.Map(fromPattern, ctx =>
        {
            if (ctx.Request.Method != HttpMethods.Get)
                return Task.CompletedTask;

            ctx.Response.StatusCode = (int)HttpStatusCode.Found;
            ctx.Response.Redirect(toPattern);
            return Task.CompletedTask;
        });
        return endpoints;
    }

    /// <summary>
    /// This extension method is used to configure MediatR in an ASP.NET Core application.
    /// It adds the necessary services to the service collection for MediatR to function properly.
    /// </summary>
    /// <param name="services">The service collection to which the MediatR services will be added.</param>
    /// <returns>The same service collection instance with the MediatR services added.</returns>
    public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration());

        return services;
    }

    /// <summary>
    /// Enables PII (Personally Identifiable Information) logging in the IdentityModelEventSource.
    /// </summary>
    /// <param name="services">The service collection to add the extension method to.</param>
    /// <returns>The same service collection instance so that multiple calls can be chained.</returns>
    public static IServiceCollection EnablePiiLogging(this IServiceCollection services)
    {
        IdentityModelEventSource.ShowPII = true;
        return services;
    }

    /// <summary>
    /// Adds global exception handling to the application's service collection.
    /// </summary>
    /// <param name="services">The service collection to add the extension method to.</param>
    /// <returns>The same service collection instance so that multiple calls can be chained.</returns>
    public static IServiceCollection ApplyGlobalExceptions(this IServiceCollection services)
    {
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<InternalServerExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Adds rate limiter configurations to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the rate limiter configurations to.</param>
    /// <returns>The same service collection instance so that multiple calls can be chained.</returns>
    public static IServiceCollection AddRateLimiterConfigurations(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Fixed Window Limiter
            options.AddFixedWindowLimiter("FixedWindowPolicy", policy =>
            {
                policy.PermitLimit = 100; // Max 100 requests
                policy.Window = TimeSpan.FromMinutes(1); // Per minute
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                policy.QueueLimit = 5; // Additional queue slots
            });

            // Sliding Window Limiter
            options.AddSlidingWindowLimiter("SlidingWindowPolicy", policy =>
            {
                policy.PermitLimit = 100; // Allow 100 requests
                policy.Window = TimeSpan.FromMinutes(1); // Per minute
                policy.SegmentsPerWindow = 5; // Divide into 5 segments
            });

            // Token Bucket Limiter
            options.AddTokenBucketLimiter("TokenBucketPolicy", policy =>
            {
                policy.TokenLimit = 10; // Maximum of 10 tokens
                policy.TokensPerPeriod = 5; // Replenish 5 tokens
                policy.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // Every 10 seconds
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                policy.QueueLimit = 3; // Allow 3 queued requests
            });

            // Concurrency Limiter
            options.AddConcurrencyLimiter("ConcurrencyPolicy", policy =>
            {
                policy.PermitLimit = 3; // Allow 3 concurrent requests
                policy.QueueLimit = 2; // Allow 2 queued requests
                policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}