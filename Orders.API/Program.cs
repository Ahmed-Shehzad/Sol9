using System.Diagnostics;

using Asp.Versioning;

using FluentResults;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using Orders.API;
using Orders.Infrastructure;
using Orders.Infrastructure.Contexts;

using Serilog;

using Sol9.ServiceDefaults;

using Transponder;
using Transponder.OpenTelemetry;
using Transponder.Persistence.EntityFramework.PostgreSql;
using Transponder.Persistence.EntityFramework.PostgreSql.Abstractions;
using Transponder.Persistence.Redis;
using Transponder.Serilog;
using Transponder.Transports.Grpc;

using Cysharp.Serialization.Json;

using Sol9.ServiceDefaults.DeadLetter;
using Sol9.Core.Serialization;

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

// Add services to the container.
builder.AddServiceDefaults();
builder.AddPostgresDeadLetterQueue();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new UlidJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

builder.Services.AddGrpc(options => options.Interceptors.Add<GrpcTransportServerInterceptor>());

builder.Services.AddInfrastructure(builder.Configuration);
ConfigureTransponder(builder);

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
builder.Services.AddOpenApi("v1", options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1);
builder.Services.AddOpenApi("v2", options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1);

builder.Services.AddOutputCache();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.EnsureDatabaseCreatedAndMigratedAsync<OrdersDbContext>().ConfigureAwait(false);

    IDbContextFactory<PostgreSqlTransponderDbContext>? transponderFactory =
        app.Services.GetService<IDbContextFactory<PostgreSqlTransponderDbContext>>();
    if (transponderFactory is not null)
        await transponderFactory.EnsureDatabaseCreatedAndMigratedAsync().ConfigureAwait(false);
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

        _ = Activity.Current?.AddException(exception);
        _ = Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);

        (Result result, ProblemDetails problemDetails, int statusCode) = exception switch
        {
            ValidationException validationException => BuildValidationProblemDetails(validationException, context),
            _ => BuildUnexpectedProblemDetails(exception, context, includeDetails)
        };

        if (logger.IsEnabled(LogLevel.Error))
            logger.LogError("Error result: {@Result}", result);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    // Map standard YAML endpoints: /openapi/v1.yaml, /openapi/v2.yaml
    _ = app.MapOpenApi("/openapi/{documentName}.yaml");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) _ = app.MapOpenApi();

bool allowUnsecuredTransport = string.Equals(
    Environment.GetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT"),
    "true",
    StringComparison.OrdinalIgnoreCase);
if (!allowUnsecuredTransport) _ = app.UseHttpsRedirection();

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
        Detail = "The server encountered an unexpected error.",
        Instance = context.Request.Path
    };
    if (includeDetails) problemDetails.Extensions["stackTrace"] = exception.ToString();
    return (result, problemDetails, StatusCodes.Status500InternalServerError);
}

static void ConfigureTransponder(WebApplicationBuilder builder)
{
    TransponderSettings settings = LoadTransponderSettings(builder.Services, builder.Configuration);
    string defaultLocal = Environment.GetEnvironmentVariable("TransponderDefaults__LocalAddress")
                          ?? builder.Configuration["TransponderDefaults:LocalAddress"]
                          ?? "http://localhost:5296";
    string defaultRemote = Environment.GetEnvironmentVariable("TransponderDefaults__RemoteAddress")
                           ?? builder.Configuration["TransponderDefaults:RemoteAddress"]
                           ?? "http://localhost:5187";

    (Uri localAddress, RemoteAddressResolution remoteResolution) = settings.ResolveAddresses(defaultLocal, defaultRemote);
    Log.Information(
        "Transponder resolved: local={LocalAddress} remote={RemoteAddresses} defaultLocal={DefaultLocal} defaultRemote={DefaultRemote} settingsLocalBase={SettingsLocalBase} settingsRemoteBase={SettingsRemoteBase}",
        localAddress,
        string.Join(",", remoteResolution.Addresses),
        defaultLocal,
        defaultRemote,
        settings.LocalBaseAddress,
        settings.RemoteBaseAddress);
    Console.WriteLine(
        $"Transponder resolved: local={localAddress} remote={string.Join(",", remoteResolution.Addresses)} " +
        $"defaultLocal={defaultLocal} defaultRemote={defaultRemote} settingsLocalBase={settings.LocalBaseAddress} " +
        $"settingsRemoteBase={settings.RemoteBaseAddress}");

    _ = builder.Services.UseSerilog();
    _ = builder.Services.UseOpenTelemetry();

    ConfigureTransponderRedis(builder);
    ConfigureTransponderPersistence(builder);
    ConfigureTransponderBus(builder, localAddress, remoteResolution);
}

static void ConfigureTransponderRedis(WebApplicationBuilder builder)
{
    string? redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrWhiteSpace(redisConnection)) return;

    _ = builder.Services.AddTransponderRedisCache(options =>
    {
        options.ConnectionString = redisConnection;
        options.AllowUntrustedCertificates = builder.Environment.IsDevelopment();
    });
}

static void ConfigureTransponderPersistence(WebApplicationBuilder builder)
{
    string? transponderConnection = builder.Configuration.GetConnectionString("Transponder");
    if (string.IsNullOrWhiteSpace(transponderConnection)) return;

    string? schema = GetTransponderSchema(builder.Configuration);
    _ = builder.Services.AddSingleton<IPostgreSqlStorageOptions>(_ => new PostgreSqlStorageOptions(schema: schema));
    string migrationsAssembly = typeof(PostgreSqlTransponderDbContext).Assembly.GetName().Name
                                ?? "Transponder.Persistence.EntityFramework.PostgreSql";
    _ = builder.Services.AddDbContextFactory<PostgreSqlTransponderDbContext>(options =>
        _ = options.UseNpgsql(transponderConnection, npgsql =>
            _ = npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema)
                .MigrationsAssembly(migrationsAssembly)));
    _ = builder.Services.AddTransponderPostgreSqlPersistence();
}

static void ConfigureTransponderBus(
    WebApplicationBuilder builder,
    Uri localAddress,
    RemoteAddressResolution remoteResolution)
{
    _ = builder.Services.AddTransponder(localAddress, options =>
    {
        _ = options.TransportBuilder.UseGrpc(localAddress, remoteResolution.Addresses);
        options.RequestAddressResolver = TransponderRequestAddressResolver.Create(
            remoteResolution.Addresses,
            remoteResolution.Strategy,
            options.RequestPathPrefix,
            options.RequestPathFormatter);
        var dlqSettings =
            PostgresDeadLetterQueueSettings.FromConfiguration(builder.Configuration);
        _ = options.UseOutbox(outbox =>
        {
            if (dlqSettings is not null) outbox.DeadLetterAddress = dlqSettings.Address;
        });
        _ = options.UsePersistedMessageScheduler(scheduler =>
        {
            if (dlqSettings is not null) scheduler.DeadLetterAddress = dlqSettings.Address;
        });
    });

    _ = builder.Services.AddHostedService<TransponderBusHostedService>();
}

static string? GetTransponderSchema(IConfiguration configuration)
{
    string? schema = configuration["TransponderPersistence:Schema"];
    return string.IsNullOrWhiteSpace(schema) ||
        string.Equals(schema, "public", StringComparison.OrdinalIgnoreCase)
        ? null
        : schema;
}

static TransponderSettings LoadTransponderSettings(IServiceCollection services, IConfiguration configuration)
{
    TransponderSettings settings = configuration.GetSection("TransponderSettings").Get<TransponderSettings>()
                                  ?? new TransponderSettings();
    _ = services.AddSingleton(settings);
    return settings;
}
