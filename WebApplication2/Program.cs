using Microsoft.AspNetCore.Server.Kestrel.Core;

using Transponder;
using Transponder.Samples;
using Transponder.Transports.Grpc;

using WebApplication2;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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

builder.Services.AddTransponder(localAddress, options =>
{
    options.RequestAddressResolver = TransponderRequestAddressResolver.Create(remoteAddresses, remoteResolution.Strategy);
    options.TransportBuilder.UseGrpc(localAddress, remoteAddresses);

    Uri pingRequestAddress = TransponderRequestAddressResolver.Create(localAddress)(typeof(PingRequest))
        ?? throw new InvalidOperationException("Ping request address could not be resolved.");

    options.TransportBuilder.UseSagaOrchestration(sagas =>
    {
        sagas.AddSaga<PingSaga, PingState>(cfg => cfg.StartWith<PingRequest>(pingRequestAddress));
    });
});

builder.Services.AddHostedService<TransponderHostedService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapGrpcService<GrpcTransportService>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

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
