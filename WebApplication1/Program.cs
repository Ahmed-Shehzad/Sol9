using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Transponder;
using Transponder.Persistence;
using Transponder.Persistence.EntityFramework.PostgreSql;
using Transponder.Persistence.EntityFramework.PostgreSql.Abstractions;
using Transponder.Persistence.EntityFramework.SqlServer;
using Transponder.Persistence.EntityFramework.SqlServer.Abstractions;
using Transponder.Samples;
using Transponder.Transports.Grpc;

using WebApplication1;
using WebApplication1.Application;
using WebApplication1.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
const string openApiDocumentName = "v1";
builder.Services.AddOpenApi(openApiDocumentName);
builder.Services.TryAddSingleton(sp =>
{
    Type? providerType = Type.GetType(
        "Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi") ?? throw new InvalidOperationException("OpenApiDocumentService type not found.");
    return (IOpenApiDocumentProvider)ActivatorUtilities.CreateInstance(
        sp,
        providerType,
        openApiDocumentName);
});
builder.Services.AddGrpc();

AddTransponderPersistence(builder.Services, builder.Configuration);

TransponderSettings transponderSettings = builder.Services.AddTransponderSettings(builder.Configuration);
Uri localBaseAddress = transponderSettings.ResolveLocalBaseAddress("https://localhost:7154");
Uri localGrpcAddress = transponderSettings.ResolveGrpcAddress(localBaseAddress);

builder.WebHost.ConfigureKestrel(options =>
{
    ConfigureGrpcEndpoints(options, localBaseAddress, localGrpcAddress);
});

(Uri localAddress, RemoteAddressResolution remoteResolution) = transponderSettings.ResolveAddresses(
    defaultLocalAddress: "https://localhost:7154",
    defaultRemoteAddress: "https://localhost:7111");

IReadOnlyList<Uri> remoteAddresses = remoteResolution.Addresses;
RemoteAddressStrategy remoteAddressStrategy = remoteResolution.Strategy;
Uri integrationEventAddress = ResolveIntegrationEventAddress(remoteAddresses);

builder.Services
    .AddWebApplication1Application()
    .AddWebApplication1Infrastructure(options => options.DestinationAddress = integrationEventAddress)
    .AddTransponder(localAddress, options =>
    {
        options.RequestAddressResolver = TransponderRequestAddressResolver.Create(remoteAddresses, remoteAddressStrategy);

        Uri pingRequestAddress = TransponderRequestAddressResolver.Create(localAddress)(typeof(PingRequest))
            ?? throw new InvalidOperationException("Ping request address could not be resolved.");

        _ = options.UseSagaOrchestration(sagas =>
        {
            _ = sagas.AddSaga<PingSaga, PingState>(cfg => cfg.StartWith<PingRequest>(pingRequestAddress));
        });
        _ = options.UsePersistedMessageScheduler();
        _ = options.UseOutbox();
    })
    .UseGrpc(localAddress, remoteAddresses);

builder.Services.AddHostedService<TransponderHostedService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) _ = app.MapOpenApi();

app.MapGrpcService<GrpcTransportService>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

static void AddTransponderPersistence(IServiceCollection services, IConfiguration configuration)
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    string? postgresConnection = configuration.GetConnectionString("TransponderPostgres");
    if (!string.IsNullOrWhiteSpace(postgresConnection))
    {
        _ = services.AddDbContextFactory<PostgreSqlTransponderDbContext>(options =>
            options.UseNpgsql(postgresConnection));
        _ = services.AddSingleton<IPostgreSqlStorageOptions>(new PostgreSqlStorageOptions());
        _ = services.AddTransponderPostgreSqlPersistence();
        return;
    }

    string? sqlConnection = configuration.GetConnectionString("TransponderSqlServer");
    if (!string.IsNullOrWhiteSpace(sqlConnection))
    {
        _ = services.AddDbContextFactory<SqlServerTransponderDbContext>(options =>
            options.UseSqlServer(sqlConnection));
        _ = services.AddSingleton<ISqlServerStorageOptions>(new SqlServerStorageOptions());
        _ = services.AddTransponderSqlServerPersistence();
        return;
    }

    _ = services.AddTransponderInMemoryPersistence();
}

static Uri ResolveIntegrationEventAddress(IReadOnlyList<Uri> remoteAddresses)
{
    if (remoteAddresses.Count == 0)
        throw new InvalidOperationException("Remote address is required for integration events.");

    Uri remote = remoteAddresses[0];
    var builder = new UriBuilder(remote)
    {
        Path = $"{remote.AbsolutePath.TrimEnd('/')}/events/orders"
    };

    return builder.Uri;
}

static void ConfigureGrpcEndpoints(KestrelServerOptions options, Uri httpAddress, Uri grpcAddress)
{
    if (string.Equals(httpAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
    {
        options.ConfigureEndpointDefaults(endpointOptions =>
        {
            endpointOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        });
        return;
    }

    if (httpAddress.Port == grpcAddress.Port)
    {
        Listen(httpAddress.Port, Http1And2);
        return;

        void Http1And2(ListenOptions listen) => listen.Protocols = HttpProtocols.Http1AndHttp2;
    }

    Listen(httpAddress.Port, Http1);
    Listen(grpcAddress.Port, Http2);
    return;

    void Http1(ListenOptions listen) => listen.Protocols = HttpProtocols.Http1;

    void Http2(ListenOptions listen) => listen.Protocols = HttpProtocols.Http2;

    void Listen(int port, Action<ListenOptions> configure)
    {
        if (httpAddress.IsLoopback)
            options.ListenLocalhost(port, configure);
        else
            options.ListenAnyIP(port, configure);
    }
}
