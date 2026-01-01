using System;

using Asp.Versioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orders.API;
using Orders.Infrastructure;

using Serilog;

using Sol9.ServiceDefaults;

using Transponder;
using Transponder.OpenTelemetry;
using Transponder.Persistence.EntityFramework.PostgreSql;
using Transponder.Persistence.EntityFramework.PostgreSql.Abstractions;
using Transponder.Persistence.Redis;
using Transponder.Serilog;
using Transponder.Transports.Grpc;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddGrpc();

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

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Map standard JSON endpoints: /openapi/v1.json, /openapi/v2.json
    _ = app.MapOpenApi();

    // Map custom YAML endpoints: /openapi/v1.yaml, /openapi/v2.yaml
    _ = app.MapOpenApi("/openapi/{documentName}.yaml");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) _ = app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<GrpcTransportService>();
app.MapDefaultEndpoints();
app.MapControllers();

await app.RunAsync();

static void ConfigureTransponder(WebApplicationBuilder builder)
{
    TransponderSettings settings = LoadTransponderSettings(builder.Services, builder.Configuration);
    string defaultLocal = builder.Configuration["TransponderDefaults:LocalAddress"] ?? "http://localhost:5296";
    string defaultRemote = builder.Configuration["TransponderDefaults:RemoteAddress"] ?? "http://localhost:5187";

    (Uri localAddress, RemoteAddressResolution remoteResolution) = settings.ResolveAddresses(defaultLocal, defaultRemote);

    _ = builder.Services.UseSerilog();
    _ = builder.Services.UseOpenTelemetry();

    string? redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConnection))
        _ = builder.Services.AddTransponderRedisCache(options =>
        {
            options.ConnectionString = redisConnection;
        });

    string? transponderConnection = builder.Configuration.GetConnectionString("Transponder");
    if (!string.IsNullOrWhiteSpace(transponderConnection))
    {
        string schema = builder.Configuration["TransponderPersistence:Schema"] ?? "orders_transponder";
        _ = builder.Services.AddSingleton<IPostgreSqlStorageOptions>(_ => new PostgreSqlStorageOptions(schema: schema));
        string migrationsAssembly = typeof(Program).Assembly.GetName().Name ?? "Orders.API";
        _ = builder.Services.AddDbContextFactory<PostgreSqlTransponderDbContext>(options =>
            _ = options.UseNpgsql(transponderConnection, npgsql =>
                _ = npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema)
                    .MigrationsAssembly(migrationsAssembly)));
        _ = builder.Services.AddTransponderPostgreSqlPersistence();
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
