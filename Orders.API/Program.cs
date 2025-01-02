using System.Reflection;
using BuildingBlocks.Contracts.Services.Tenants;
using BuildingBlocks.Contracts.Services.Users;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.ServiceCollection.Extensions;
using BuildingBlocks.Utilities.Behaviors;
using BuildingBlocks.Utilities.Middlewares;
using CorrelationId;
using CorrelationId.DependencyInjection;
using CorrelationId.Providers;
using Hangfire;
using HealthChecks.UI.Client;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Orders.API.Configurations;
using Orders.Application.Extensions;
using Serilog;
using Serilog.Enrichers.AspNetCore;
using StackExchange.Redis;
using static BuildingBlocks.ServiceCollection.Extensions.LoggerConfigurationExtension;

namespace Orders.API;

/// <summary>
/// The main entry point for the application.
/// </summary>
public class Program
{
    private static readonly string[] Tags = ["ordersdb"];

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Scan all assemblies in the current AppDomain
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var builder = WebApplication.CreateBuilder(args);

        // Access the configuration and environment services.
        var configuration = builder.Configuration;
        var environment = builder.Environment;
        var isDockerEnvironment = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true",
            StringComparison.OrdinalIgnoreCase);

        configuration.AddJsonFile("appsettings.json", true, true);

        if (environment.IsDevelopment() || environment.IsStaging() || environment.IsProduction())
        {
            configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        }

        configuration.AddEnvironmentVariables(); // This ensures environment variables are used   

        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                var defaultOptions = JsonConfigurations.GetDefaultOptions();
                options.JsonSerializerOptions.Converters.AddRange(defaultOptions.Converters);
                options.JsonSerializerOptions.DefaultIgnoreCondition = defaultOptions.DefaultIgnoreCondition;
                options.JsonSerializerOptions.AllowTrailingCommas = defaultOptions.AllowTrailingCommas;
                options.JsonSerializerOptions.WriteIndented = defaultOptions.WriteIndented;
                options.JsonSerializerOptions.PropertyNamingPolicy = defaultOptions.PropertyNamingPolicy;
            });

        builder.Services.AddAuthorization().AddKeycloakAuthorization();

        builder.Services.AddKeycloakWebApiAuthentication(options =>
        {
            options.Audience = configuration["Keycloak:Audience"]!;
            options.Realm = configuration["Keycloak:Realm"]!;
            options.AuthServerUrl = configuration["Keycloak:KeycloakServerUrl"]!;
            options.TokenClockSkew = TimeSpan.Zero;
            options.VerifyTokenAudience = true;
            options.Credentials.Secret = configuration["Keycload:ClientSecret"]!;
        }, options =>
        {
            options.RequireHttpsMetadata = false;
            options.Audience = configuration["Keycloak:Audience"]!;
            options.MetadataAddress = configuration["Keycloak:WellKnownConfiguration"]!;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = $"{configuration["Keycloak:KeycloakServerUrl"]!}/{configuration["Keycloak:Realm"]!}",
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.FormFieldName = "__RequestVerificationToken";
            options.Cookie.Name = "__Host-X-XSRF-TOKEN";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.Expiration = TimeSpan.FromDays(1);
            options.Cookie.IsEssential = false;
            options.Cookie.MaxAge = TimeSpan.FromDays(1);
            options.SuppressXFrameOptionsHeader = true;
        });

        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
            options.CombineLogs = true;
        });

        builder.Services.AddOutputCache(options =>
        {
            options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(30);
            options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(30)));
        });

        // Add API versioning.
        builder.Services.AddApiVersionings();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

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

        // builder.Services.AddTreblle(
        //     builder.Configuration["Treblle:ApiKey"]!,
        //     builder.Configuration["Treblle:ProjectId"]!,
        //     "secretField,highlySensitiveField");

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        // Register response compression services
        builder.Services.AddResponseCompression();

        builder.Services.AddHttpContextMiddleware();

        builder.Host.UseSerilog(GetSerilogLoggerConfiguration(environment));

        builder.Services.AddRateLimiterConfigurations();
        builder.Services.AddMediatRConfiguration();

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
                AbortOnConnectFail = false,
                EndPoints =
                {
                    options.Configuration!
                }
            };
        });

        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddNpgSql(connectionString: configuration.GetConnectionString("DefaultConnection")!,
                name: "orders-db",
                tags: Tags);

        builder.Services.AddServiceLogEnricher();

        builder.Services.AddScoped<ITenantService, TenantService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

        builder.Services.AddDbContexts(configuration);
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices();

        builder.Services.AddOpenTelemetry("Orders.API");

        builder.Services.AddOutboxTransactions(configuration);

        builder.Services.ConfigureDefaultUnauthorizedBehavior();

        builder.Services.AddTransient<AcceptHeaderMiddleware>();
        builder.Services.AddTransient<AllowHeaderMiddleware>();
        builder.Services.AddTransient<SecurityHeadersMiddleware>();
        builder.Services.AddTransient<TenantMiddleware>();
        builder.Services.AddTransient<UserMiddleware>();
        
        var app = builder.Build();

        // if (environment.IsDevelopment())
        // {
        //     // Seed the database
        //     using var scope = app.Services.CreateScope();
        //     var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //     var unitOfwork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        //     
        //     var orders = OrderGenerator.GenerateMinimumTenOrders();
        //     
        //     orderRepo.AddRange(orders);
        //     unitOfwork.CommitAsync().GetAwaiter().GetResult();
        // }

        app.UseStatusCodePages();
        app.UseExceptionHandler();
        app.UseDeveloperExceptionPage();

        if (!string.IsNullOrEmpty(configuration["BasePath"]))
        {
            app.UsePathBase(configuration["BasePath"]);
        }

        var rewriteOptions = new RewriteOptions().AddRewrite("(.*)/$", "$1", true);

        app.UseRewriter(rewriteOptions);

        // // Schedule the job
        // RecurringJob.AddOrUpdate<OutboxProcessorJob>(
        //     "process-orders-api-outbox-messages",
        //     job => job.ExecuteAsync(),
        //     Cron.Minutely); // Adjust the Cron expression as needed

        // Configure the HTTP request pipeline.
        app.UseSwagger(options =>
        {
            options.SerializeAsV2 = true;
        });
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "Sol9 - Orders HTTP API";
            options.DisplayRequestDuration();
            options.EnablePersistAuthorization();
            options.DisplayOperationId();
            options.ShowExtensions();
            options.ShowCommonExtensions();
            
            var descriptions = app.DescribeApiVersions().Reverse();
            foreach (var description in descriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.yaml",
                    description.GroupName.ToUpperInvariant());
            }
        });

        app.UseForwardedHeaders();

        app.UseCorrelationId();

        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors("AllowAll");

        app.UseRequestLocalization();

        app.UseAuthentication(); // Authentication middleware

        app.UseAuthorization(); // Authorization middleware

        app.UseRateLimiter();

        app.UseAntiforgery();

        app.UseOutputCache();

        app.UseHttpContextMiddleware();

        app.UseHttpLogging();

        app.UseMiddleware<AcceptHeaderMiddleware>();
        app.UseMiddleware<AllowHeaderMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<TenantMiddleware>();
        app.UseMiddleware<UserMiddleware>();

        app.UseResponseCaching();
        app.UseResponseCompression();

        app.UseSerilogRequestLogging();

        app.UseSerilogPlusRequestLogging();

        if (environment.IsDevelopment() || isDockerEnvironment)
        {
            app.MapDevTools("/dev", options =>
                options.AddEntry("Configuration", sp => sp.GetService<IConfiguration>()!.Expand()));
        }

        // Health check routes
        app.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self")
        });
        
        // Run Hangfire server
        app.MapHangfireDashboard(pattern: "/hangfire", options: new DashboardOptions
        {
            IgnoreAntiforgeryToken = environment.IsDevelopment(),
        });

        app.MapSwagger();

        // Map controllers
        app.MapControllers();

        app.Run();
    }
}