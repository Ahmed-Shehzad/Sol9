using Bookings.API;
using Bookings.Application.Transponder;
using Bookings.Infrastructure;
using Bookings.Infrastructure.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi;

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
builder.Services.AddGrpc();

builder.Services.AddInfrastructure(builder.Configuration);

ConfigureTransponder(builder);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using IServiceScope scope = app.Services.CreateScope();
    BookingsDbContext dbContext = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
    await dbContext.Database.MigrateAsync().ConfigureAwait(false);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
    _ = app.MapGet("/openapi/v1.yaml", async (HttpContext context, IOpenApiDocumentProvider provider) =>
    {
        context.Response.ContentType = "application/yaml";
        OpenApiDocument document = await provider.GetOpenApiDocumentAsync(context.RequestAborted);
        await using var writer = new StreamWriter(context.Response.Body);
        document.SerializeAsV31(new OpenApiYamlWriter(writer));
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<GrpcTransportService>();
app.MapDefaultEndpoints();
app.MapControllers();

await app.RunAsync();

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
        });

    string? transponderConnection = builder.Configuration.GetConnectionString("Transponder");
    if (!string.IsNullOrWhiteSpace(transponderConnection))
    {
        string schema = builder.Configuration["TransponderPersistence:Schema"] ?? "bookings_transponder";
        _ = builder.Services.AddSingleton<IPostgreSqlStorageOptions>(_ => new PostgreSqlStorageOptions(schema: schema));
        _ = builder.Services.AddDbContextFactory<PostgreSqlTransponderDbContext>(options => options.UseNpgsql(transponderConnection));
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
