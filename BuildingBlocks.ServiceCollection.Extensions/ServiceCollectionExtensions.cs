using System.Net;
using System.Reflection;
using System.Threading.RateLimiting;
using Asp.Versioning;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.Utilities.Exceptions.Handlers;
using BuildingBlocks.Utilities.Filters.Swashbuckle;
using BuildingBlocks.Utilities.Types;
using FluentValidation;
using MediatR.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.ServiceCollection.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Extension method to configure API versioning for the service collection.
    /// </summary>
    /// <param name="service">The service collection to add the API versioning to.</param>
    /// <returns>The same service collection instance so that multiple calls can be chained.</returns>
    public static IServiceCollection UseApiVersioning(this IServiceCollection service)
    {
        service.AddApiVersioning(option =>
        {
            option.AssumeDefaultVersionWhenUnspecified =
                true; //This ensures if client doesn't specify an API version. The default version should be considered. 
            option.DefaultApiVersion = new ApiVersion(1, 0); //This we set the default API version
            option.ReportApiVersions =
                true; //The allow the API Version information to be reported in the client  in the response header. This will be useful for the client to understand the version of the API they are interacting with.
            //------------------------------------------------//
            option.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("X-Api-Version"),
                new HeaderApiVersionReader("X-Version"),
                new MediaTypeApiVersionReader(
                    "ver")); //This says how the API version should be read from the client's request, 3 options are enabled 1.Querystring, 2.Header, 3.MediaType. 
            //"api-version", "X-Version" and "ver" are parameter name to be set with version number in client before request the endpoints.
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; //The say our format of our version number “‘v’major[.minor][-status]”
            options.SubstituteApiVersionInUrl =
                true; //This will help us to resolve the ambiguity when there is a routing conflict due to routing template one or more end points are same.
        });
        return service;
    }

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
    public static IServiceCollection AddGlobalExceptions(this IServiceCollection services)
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
        services.AddRateLimiter(rateLimiterOptions =>
            rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
            {
                options.PermitLimit = 4;
                options.Window = TimeSpan.FromSeconds(12);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            }).AddSlidingWindowLimiter("sliding", options =>
            {
                options.PermitLimit = 100;
                options.Window = TimeSpan.FromSeconds(10);
                options.SegmentsPerWindow = 8;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            }).AddTokenBucketLimiter("token", options =>
            {
                options.TokenLimit = 10;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
                options.ReplenishmentPeriod = TimeSpan.FromSeconds(2);
                options.TokensPerPeriod = 4;
                options.AutoReplenishment = false;
            }).AddConcurrencyLimiter("Concurrency", options =>
            {
                options.PermitLimit = 4;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            }));
        return services;
    }

    /// <summary>
    /// Adds Swagger generation services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the Swagger generation services to.</param>
    /// <param name="assembly">The assembly containing the XML comments for the Swagger documentation.</param>
    /// <returns>The same service collection instance so that multiple calls can be chained.</returns>
    public static IServiceCollection UseSwagger(this IServiceCollection services, Assembly assembly)
    {
        services.AddSwaggerGen(options =>
        {
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.UseAllOfForInheritance();
            options.UseOneOfForPolymorphism();
            options.UseInlineDefinitionsForEnums();
            options.SchemaFilter<UlidSchemaFilter>();
            options.SelectDiscriminatorNameUsing(_ => "discriminator");
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }

    /// <summary>
    /// Adds FluentValidation services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="assembly"></param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method adds all the validators found in the assembly of the calling class to the service collection.
    /// </remarks>
    public static IServiceCollection AddFluentValidation(this IServiceCollection services, Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry tracing and logging services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the OpenTelemetry services to.</param>
    /// <param name="serviceName">The name of the service to be used in the OpenTelemetry resource.</param>
    /// <returns>The same service collection instance so that multiple calls can be chained.</returns>
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        // Configure OpenTelemetry tracing
        services.AddOpenTelemetry()
            .WithTracing(providerBuilder =>
            {
                providerBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRedisInstrumentation(options =>
                    {
                        options.EnrichActivityWithTimingEvents = true;
                        options.SetVerboseDatabaseStatements = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.EnableConnectionLevelAttributes = true;
                        options.RecordException = true;
                    })
                    .AddNpgsql()
                    .AddOtlpExporter();
            })
            .WithLogging(providerBuilder =>
            {
                providerBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
                providerBuilder.AddOtlpExporter();
            });
        return services;
    }
}