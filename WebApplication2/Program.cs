using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Transponder;
using Transponder.Contracts.Orders;
using Transponder.Samples;
using Transponder.Transports.Grpc;

using WebApplication2;
using WebApplication2.Application;
using WebApplication2.Application.Orders;
using WebApplication2.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
const string openApiDocumentName = "v1";
builder.Services.AddOpenApi(openApiDocumentName);
builder.Services.TryAddSingleton<Microsoft.AspNetCore.OpenApi.IOpenApiDocumentProvider>(sp =>
{
    Type? providerType = Type.GetType(
        "Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi");
    if (providerType is null)
        throw new InvalidOperationException("OpenApiDocumentService type not found.");

    return (Microsoft.AspNetCore.OpenApi.IOpenApiDocumentProvider)ActivatorUtilities.CreateInstance(
        sp,
        providerType,
        openApiDocumentName);
});
builder.Services.AddGrpc();

TransponderSettings transponderSettings = builder.Services.AddTransponderSettings(builder.Configuration);
Uri localBaseAddress = transponderSettings.ResolveLocalBaseAddress("https://localhost:7111");
Uri localGrpcAddress = transponderSettings.ResolveGrpcAddress(localBaseAddress);

builder.WebHost.ConfigureKestrel(options =>
{
    ConfigureGrpcEndpoints(options, localBaseAddress, localGrpcAddress);
});

(Uri localAddress, RemoteAddressResolution remoteResolution) = transponderSettings.ResolveAddresses(
    defaultLocalAddress: "https://localhost:7111",
    defaultRemoteAddress: "https://localhost:7154");

IReadOnlyList<Uri> remoteAddresses = remoteResolution.Addresses;
Uri integrationEventAddress = ResolveIntegrationEventAddress(localAddress);

builder.Services
    .AddWebApplication2Application()
    .AddWebApplication2Infrastructure()
    .AddTransponder(localAddress, options =>
    {
        options.RequestAddressResolver = TransponderRequestAddressResolver.Create(remoteAddresses, remoteResolution.Strategy);

        Uri pingRequestAddress = TransponderRequestAddressResolver.Create(localAddress)(typeof(PingRequest))
            ?? throw new InvalidOperationException("Ping request address could not be resolved.");

        _ = options.UseSagaOrchestration(sagas =>
        {
            _ = sagas.AddSaga<PingSaga, PingState>(cfg => cfg.StartWith<PingRequest>(pingRequestAddress));
            _ = sagas.AddSaga<OrderIntegrationSaga, OrderIntegrationSagaState>(
                cfg => cfg.StartWith<OrderCreatedIntegrationEvent>(integrationEventAddress));
            _ = sagas.AddSaga<OrderFollowUpSaga, OrderFollowUpSagaState>(
                cfg => cfg.StartWith<OrderFollowUpScheduledIntegrationEvent>(integrationEventAddress));
        });
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

static Uri ResolveIntegrationEventAddress(Uri localAddress)
{
    var builder = new UriBuilder(localAddress)
    {
        Path = $"{localAddress.AbsolutePath.TrimEnd('/')}/events/orders"
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
