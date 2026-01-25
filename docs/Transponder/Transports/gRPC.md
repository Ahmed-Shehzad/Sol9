# gRPC Transport

The gRPC transport provides high-performance, binary protocol communication for service-to-service messaging in Transponder.

## Overview

gRPC transport is ideal for:
- Service-to-service communication
- Request/response patterns
- High-performance requirements
- HTTP/2 benefits (multiplexing, header compression)

## When to Use

**Use gRPC transport when:**
- You need high-performance inter-service communication
- You're building microservices or modular monoliths
- You need request/response patterns
- You want HTTP/2 benefits
- Services are in the same network or trusted environment

**Don't use gRPC transport when:**
- You need browser-based clients (use SignalR or SSE)
- You need webhook delivery (use Webhooks transport)
- You need event streaming (use Kafka transport)

## Getting Started

### 1. Install Package

```bash
dotnet add package Transponder.Transports.Grpc
```

### 2. Shared Contracts

The gRPC `.proto` contract lives in a shared project so all services compile against the same schema:

- `Transponder.Transports.Grpc.Contracts/Protos/transponder_transport.proto`

Each service hosts its own gRPC endpoints; the shared project is only for contract + codegen.

### 3. Register Services

```csharp
using Transponder;
using Transponder.Transports.Grpc;

builder.Services.AddTransponder(
    new Uri("http://localhost:5187"),
    options =>
    {
        options.TransportBuilder.UseGrpc(
            localAddress: new Uri("http://localhost:5187"),
            remoteAddresses: new[] 
            { 
                new Uri("http://localhost:5296"),
                new Uri("http://localhost:5396")
            });
    });

// Enable gRPC + interceptors
builder.Services.AddGrpc(options => options.Interceptors.Add<GrpcTransportServerInterceptor>());

// The interceptor enforces required fields, injects correlation IDs, and validates content type.

var app = builder.Build();

// Map gRPC service
app.MapGrpcService<GrpcTransportService>();
```

### 4. Configure Kestrel for HTTP/2

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listener => 
        listener.Protocols = HttpProtocols.Http1AndHttp2);
    
    // Or configure specific endpoint
    options.ListenLocalhost(5187, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
```

## Configuration Options

### Basic Configuration

```csharp
options.TransportBuilder.UseGrpc(
    localAddress: new Uri("http://localhost:5187"),
    remoteAddresses: new[] { new Uri("http://localhost:5296") });
```

### Advanced Configuration

```csharp
using Transponder.Transports.Abstractions;

var settings = new GrpcHostSettings(
    new Uri("https://localhost:7266"),
    maxReceiveMessageSize: 16 * 1024 * 1024,
    keepAliveTime: TimeSpan.FromSeconds(30),
    resilienceOptions: new TransportResilienceOptions
    {
        EnableRetry = true,
        EnableCircuitBreaker = true,
        Retry = new TransportRetryOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(200)
        }
    });

options.TransportBuilder.UseGrpc(settings);
```

## Use Cases

### 1. Request/Response Between Services

```csharp
// Orders.API
var client = _clientFactory.CreateRequestClient<CreateBookingRequest>();
var response = await client.GetResponseAsync<CreateBookingResponse>(
    new CreateBookingRequest(order.Id, order.CustomerName));
```

### 2. Publish Events

```csharp
// Orders.API
await _publisher.PublishAsync(new OrderCreatedEvent(order.Id));
```

### 3. Send Messages

```csharp
// Orders.API
var endpoint = await _sendEndpointProvider.GetSendEndpointAsync(
    new Uri("https://bookings-service:8080"));
await endpoint.SendAsync(new ProcessBookingCommand(bookingId));
```

## HTTPS Configuration

For production, use HTTPS:

```csharp
options.TransportBuilder.UseGrpc(
    localAddress: new Uri("https://bookings-api.example.com"),
    remoteAddresses: new[] { new Uri("https://orders-api.example.com") });
```

### Kestrel HTTPS Configuration

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 443, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        listenOptions.UseHttps("certificate.pfx", "password");
    });
});
```

## Resilience

Configure retry and circuit breaker:

```csharp
var resilience = new TransportResilienceOptions
{
    EnableRetry = true,
    EnableCircuitBreaker = true,
    Retry = new TransportRetryOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        MaxDelay = TimeSpan.FromSeconds(10)
    },
    CircuitBreaker = new TransportCircuitBreakerOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 10
    }
};

var settings = new GrpcHostSettings(
    new Uri("https://localhost:7266"),
    resilienceOptions: resilience);

options.TransportBuilder.UseGrpc(settings);
```

## Performance Tips

1. **Use HTTP/2**: Enable HTTP/2 for multiplexing and header compression
2. **Use HTTPS**: HTTPS enables HTTP/2 in browsers and improves security
3. **Connection Pooling**: gRPC reuses connections automatically
4. **Streaming**: Use streaming for large payloads
5. **Compression**: Enable compression for large messages

## Troubleshooting

### Connection Issues

```bash
# Test gRPC endpoint
grpcurl -plaintext localhost:5187 list

# Check HTTP/2
curl -v --http2 https://localhost:7266
```

### Common Issues

1. **HTTP/2 not enabled**: Ensure Kestrel is configured for HTTP/2
2. **Certificate issues**: Use proper certificates for HTTPS
3. **Firewall blocking**: Ensure ports are open
4. **DNS resolution**: Verify service names resolve correctly

## See Also

- [Transponder Core Documentation](../README.md)
- [Transport Overview](README.md)
