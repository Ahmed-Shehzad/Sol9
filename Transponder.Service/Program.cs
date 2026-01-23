using Microsoft.AspNetCore.Server.Kestrel.Core;
using Sol9.ServiceDefaults;
using Transponder.Transports;
using Transponder.Transports.Grpc;
using Transponder.Transports.Grpc.Abstractions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Kestrel for HTTP/2 (required for gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listener => listener.Protocols = HttpProtocols.Http1AndHttp2);
});

// Add gRPC services
builder.Services.AddGrpc();

// Get the gRPC endpoint URL from configuration
string? grpcUrl = Environment.GetEnvironmentVariable("Kestrel__Endpoints__Grpc__Url")
                  ?? builder.Configuration["Kestrel:Endpoints:Grpc:Url"];

if (string.IsNullOrWhiteSpace(grpcUrl)) throw new InvalidOperationException("Kestrel__Endpoints__Grpc__Url must be configured.");

if (!Uri.TryCreate(grpcUrl, UriKind.Absolute, out Uri? grpcAddress)) throw new InvalidOperationException($"Invalid gRPC URL: {grpcUrl}");

// Register Transponder gRPC transport
// The service acts as a central hub, so it only needs the local address
builder.Services.AddTransponderTransports(transportBuilder =>
{
    transportBuilder.AddTransportFactory<GrpcTransportFactory>();
    transportBuilder.AddTransportHost<IGrpcHostSettings, GrpcTransportHost>(
        _ => new GrpcHostSettings(
            grpcAddress,
            useTls: string.Equals(grpcAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)),
        (_, settings) => new GrpcTransportHost(settings));
});

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Map the gRPC transport service
app.MapGrpcService<GrpcTransportService>();

await app.RunAsync();
