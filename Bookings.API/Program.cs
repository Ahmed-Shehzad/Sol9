using System.Diagnostics;

using Bookings.API;
using Bookings.Application.Transponder;
using Bookings.Infrastructure;
using Bookings.Infrastructure.Contexts;

using FluentResults;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi;

using Asp.Versioning;

using Serilog;

using Sol9.Contracts.Bookings;
using Sol9.ServiceDefaults;

using Transponder;
using Transponder.OpenTelemetry;
using Transponder.Persistence.EntityFramework;
using Transponder.Persistence.EntityFramework.PostgreSql;
using Transponder.Persistence.EntityFramework.PostgreSql.Abstractions;
using Transponder.Persistence.Redis;
using Transponder.Serilog;
using Transponder.Transports.Grpc;

using Verifier.Exceptions;

using ILogger = Microsoft.Extensions.Logging.ILogger;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listener => listener.Protocols = HttpProtocols.Http1AndHttp2);
});

// 1. Add API Versioning Services
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        // Format the group name as 'v1', 'v2', etc.
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// 2. Register OpenAPI for each version
// Note: .NET 10 allows multiple documents by providing a unique name
builder.Services.AddOpenApi("v1", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
builder.Services.AddOpenApi("v2", options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);

// Add services to the container.
builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddGrpc();

builder.Services.AddInfrastructure(builder.Configuration);

ConfigureTransponder(builder);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using IServiceScope scope = app.Services.CreateScope();
    BookingsDbContext dbContext = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
    await dbContext.Database.MigrateAsync().ConfigureAwait(false);

    IDbContextFactory<PostgreSqlTransponderDbContext>? transponderFactory =
        scope.ServiceProvider.GetService<IDbContextFactory<PostgreSqlTransponderDbContext>>();
    if (transponderFactory is not null)
    {
        await using PostgreSqlTransponderDbContext transponderDb =
            await transponderFactory.CreateDbContextAsync().ConfigureAwait(false);
        await transponderDb.Database.MigrateAsync().ConfigureAwait(false);
    }
}

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        IExceptionHandlerFeature? feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is null) return;

        Exception exception = feature.Error;
        ILogger logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");
        logger.LogError(exception, "Unhandled exception");

        IHostEnvironment environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
        bool includeDetails = environment.IsDevelopment();

        _ = (Activity.Current?.AddException(exception));
        _ = (Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message));

        (Result result, ProblemDetails problemDetails, int statusCode) = exception switch
        {
            ValidationException validationException => BuildValidationProblemDetails(validationException, context),
            _ => BuildUnexpectedProblemDetails(exception, context, includeDetails)
        };

        logger.LogError("Error result: {@Result}", result);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Map standard JSON endpoints: /openapi/v1.json, /openapi/v2.json
    _ = app.MapOpenApi();

    // Map custom YAML endpoints: /openapi/v1.yaml, /openapi/v2.yaml
    _ = app.MapOpenApi("/openapi/{documentName}.yaml");
}

bool allowUnsecuredTransport = string.Equals(
    Environment.GetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT"),
    "true",
    StringComparison.OrdinalIgnoreCase);
if (!allowUnsecuredTransport) app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<GrpcTransportService>();
app.MapDefaultEndpoints();
app.MapControllers();

await app.RunAsync();

static (Result result, ProblemDetails problemDetails, int statusCode) BuildValidationProblemDetails(
    ValidationException exception,
    HttpContext context)
{
    var errors = exception.Errors
        .GroupBy(error => error.PropertyName)
        .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

    var result = Result.Fail(new Error("Validation failed")
        .WithMetadata("errors", errors));

    var problemDetails = new ProblemDetails
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Validation failed",
        Detail = "One or more validation errors occurred.",
        Instance = context.Request.Path,
        Extensions = { ["errors"] = errors }
    };

    return (result, problemDetails, StatusCodes.Status400BadRequest);
}

static (Result result, ProblemDetails problemDetails, int statusCode) BuildUnexpectedProblemDetails(
    Exception exception,
    HttpContext context,
    bool includeDetails)
{
    var result = Result.Fail("An unexpected error occurred.");
    var problemDetails = new ProblemDetails
    {
        Status = StatusCodes.Status500InternalServerError,
        Title = "An unexpected error occurred.",
        Detail = includeDetails ? exception.Message : "The server encountered an unexpected error.",
        Instance = context.Request.Path
    };
    if (includeDetails) problemDetails.Extensions["stackTrace"] = exception.ToString();
    return (result, problemDetails, StatusCodes.Status500InternalServerError);
}

static void ConfigureTransponder(WebApplicationBuilder builder)
{
    TransponderSettings settings = LoadTransponderSettings(builder.Services, builder.Configuration);
    string defaultLocal = builder.Configuration["TransponderDefaults:LocalAddress"] ?? "http://localhost:5187";
    string defaultRemote = builder.Configuration["TransponderDefaults:RemoteAddress"] ?? "http://localhost:5296";

    (Uri localAddress, RemoteAddressResolution remoteResolution) = settings.ResolveAddresses(defaultLocal, defaultRemote);

    _ = builder.Services.UseSerilog();
    _ = builder.Services.UseOpenTelemetry();

    string? redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConnection))
        _ = builder.Services.AddTransponderRedisCache(options =>
        {
            options.ConnectionString = redisConnection;
            options.AllowUntrustedCertificates = builder.Environment.IsDevelopment();
        });

    string? transponderConnection = builder.Configuration.GetConnectionString("Transponder");
    if (!string.IsNullOrWhiteSpace(transponderConnection))
    {
        string schema = builder.Configuration["TransponderPersistence:Schema"] ?? "bookings_transponder";
        _ = builder.Services.AddSingleton<IPostgreSqlStorageOptions>(_ => new PostgreSqlStorageOptions(schema: schema));
        string migrationsAssembly = typeof(Program).Assembly.GetName().Name ?? "Bookings.API";
        _ = builder.Services.AddDbContextFactory<PostgreSqlTransponderDbContext>(options =>
            _ = options.UseNpgsql(transponderConnection, npgsql =>
                _ = npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema)
                    .MigrationsAssembly(migrationsAssembly)));
        _ = builder.Services.AddScoped<PostgreSqlTransponderDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<PostgreSqlTransponderDbContext>>().CreateDbContext());
        _ = builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<PostgreSqlTransponderDbContext>());
        _ = builder.Services.AddTransponderPostgreSqlPersistence();
        _ = builder.Services.AddEntityFrameworkSagaRepository<CreateBookingSagaState>();
    }

    _ = builder.Services.AddTransponder(localAddress, options =>
    {
        _ = options.TransportBuilder.UseGrpc(localAddress, remoteResolution.Addresses);
        options.RequestAddressResolver = TransponderRequestAddressResolver.Create(
            remoteResolution.Addresses,
            remoteResolution.Strategy,
            options.RequestPathPrefix,
            options.RequestPathFormatter);
        _ = options.UseOutbox();
        _ = options.UsePersistedMessageScheduler();
        _ = options.UseSagaChoreography(registration =>
        {
            _ = registration.AddSaga<CreateBookingSaga, CreateBookingSagaState>(endpoint =>
            {
                Uri inputAddress = TransponderRequestAddressResolver.Create(localAddress)(typeof(CreateBookingRequest))
                    ?? throw new InvalidOperationException("Failed to resolve input address.");
                _ = endpoint.StartWith<CreateBookingRequest>(inputAddress);
            });
        });
    });

    _ = builder.Services.AddHostedService<TransponderBusHostedService>();
}

static TransponderSettings LoadTransponderSettings(IServiceCollection services, IConfiguration configuration)
{
    TransponderSettings settings = configuration.GetSection("TransponderSettings").Get<TransponderSettings>()
                                  ?? new TransponderSettings();
    _ = services.AddSingleton(settings);
    return settings;
}
