using System.Reflection;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.ServiceCollection.Extensions;
using BuildingBlocks.Utilities.Behaviors;
using BuildingBlocks.Utilities.Middlewares;
using BuildingBlocks.Utilities.Types;
using CorrelationId.DependencyInjection;
using CorrelationId.Providers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Orders.Application.Extensions;
using Orders.Infrastructure.Contexts;
using Serilog;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using Treblle.Net.Core;
using static BuildingBlocks.ServiceCollection.Extensions.LoggerConfigurationExtension;

namespace Orders.API;

public class Program
{
    private static readonly string[] Tags = ["ordersdb"];
    public static void Main(string[] args)
    {
        // Scan all assemblies in the current AppDomain
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var builder = WebApplication.CreateBuilder(args);

        // Access the configuration and environment services.
        var configuration = builder.Configuration;
        var environment = builder.Environment;

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.UseSwagger(Assembly.GetExecutingAssembly());

        builder.Services.AddOptions();
        builder.Services.AddMemoryCache();

        // Add Problem Details
        builder.Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions.Add("trace-id", ctx.HttpContext.TraceIdentifier);
                ctx.ProblemDetails.Extensions.Add("instance", $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}");
            });

        builder.Services.AddRouting(options => options.LowercaseUrls = true);
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", cors => cors
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddGlobalExceptions();

        builder.Services.EnablePiiLogging();

        builder.Services.AddTreblle(
            builder.Configuration["Treblle:ApiKey"]!,
            builder.Configuration["Treblle:ProjectId"]!,
            "secretField,highlySensitiveField");

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        // Register response compression services
        builder.Services.AddResponseCompression();
        builder.Host.UseSerilog(GetSerilogLoggerConfiguration(environment));

        builder.Services.AddRateLimiterConfigurations();
        builder.Services.AddMediatRConfiguration();
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddCorrelationId<GuidCorrelationIdProvider>(options => { options.RequestHeader = "X-Correlation-Id"; });

        // Register MediatR and scan for handlers in all assemblies
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        builder.Services.AddFluentValidation(Assembly.GetExecutingAssembly());

        // HSTS Security Headers 
        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromHours(1);
        });

        // Add Redis for caching services
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["REDIS_CONNECTION_STRING"];
            options.InstanceName = "Orders.API";
            options.ConfigurationOptions = new ConfigurationOptions
            {
                AbortOnConnectFail = true,
                EndPoints =
                {
                    options.Configuration!
                }
            };
        });

        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!,
                name: "orders-db",
                tags: Tags);

        // Add API versioning.
        builder.Services.UseApiVersioning();

        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

        builder.Services.AddDbContexts(configuration);
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices();

        builder.Services.AddOpenTelemetry("Orders.API");

        builder.Services.AddOutboxTransactions();

        builder.Services.ConfigureDefaultUnauthorizedBehavior();

        var app = builder.Build();

        // // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI();
        // }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseRateLimiter();

        app.UseMiddleware<AcceptHeaderMiddleware>();
        app.UseMiddleware<AllowHeaderMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseAuthorization();

        app.UseTreblle(useExceptionHandler: true);

        app.MapControllers();

        app.Run();
    }
}