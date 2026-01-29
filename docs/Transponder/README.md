# Transponder

Transponder is the enterprise messaging framework used in Sol9 for **inter-service and inter-module communication**. It supports request/response, publish/subscribe, sagas, outbox/inbox, and message scheduling over pluggable transports (gRPC, SignalR, Kafka, RabbitMQ, etc.).

---

## When to Use

- **Cross-boundary communication**: One module or service needs to call or notify another (e.g. Orders → Bookings via gRPC).
- **Reliable messaging**: You need outbox/inbox, at-least-once delivery, or saga orchestration/choreography.
- **Decoupled request/response**: You want type-safe request clients (`IRequestClient<TRequest>`) without tying callers to transport or location.
- **Event distribution**: Publish domain or integration events to multiple consumers (in-process or across services).
- **Scheduled messages**: Delay or schedule messages (in-memory or persisted).

---

## Where to Use

- **Application / API projects** that participate in messaging: register the bus, configure transport(s), and host receive endpoints (e.g. Bookings.API, Orders.API).
- **Application layer** (handlers): use `IClientFactory` to send requests, `IPublishEndpoint` to publish events, and saga handlers for long-running flows.
- **Not in Domain or Infrastructure** as the primary entry point: domain raises events; infrastructure may implement persistence; the API layer wires Transponder and hosts the bus.

---

## Why to Use

- **Transport-agnostic API**: Same `Send`, `Request`, `Publish` API regardless of gRPC, Kafka, or RabbitMQ.
- **Multiple transports**: Mix transports per environment (gRPC in K8s, SignalR for browsers, Kafka for event streaming).
- **Reliability**: Outbox pattern for durable sends; inbox for idempotent consumption; saga support for distributed flows.
- **Testability**: Swap transports or use in-memory options in tests.

---

## Why Not to Use

- **Single process, no cross-boundary calls**: If everything runs in one process and you only need in-process mediation, use **Intercessor** (mediator) instead; no need for Transponder.
- **Simple CRUD with no events or cross-service calls**: Adding Transponder adds complexity; use Intercessor + Verifier for commands/queries only.
- **Very high throughput, low latency with no reliability needs**: Transponder adds layers (serialization, transport, optional outbox); for ultra-low-latency in-process only, consider direct calls or Intercessor only.

---

## How to Set Up

### 1. Add the package

```bash
dotnet add package Transponder
dotnet add package Transponder.Transports.Grpc   # or SignalR, Kafka, RabbitMQ, etc.
```

For persistence (outbox, saga, scheduled messages):

```bash
dotnet add package Transponder.Persistence.EntityFramework.PostgreSql
# and optionally Transponder.Persistence.Redis for cache/coordination
```

### 2. Configure addresses

Addresses define **this** service’s listen endpoint and **remote** service(s) for request/response. Use `TransponderSettings` (from config) or environment variables.

**Option A – Configuration (appsettings / ConfigMap)**

```json
{
  "TransponderSettings": {
    "LocalBaseAddress": "http://localhost:5187",
    "RemoteBaseAddress": "http://localhost:5296"
  }
}
```

**Option B – Environment variables (e.g. Kubernetes)**

```bash
TransponderDefaults__LocalAddress=http://bookings-api:80
TransponderDefaults__RemoteAddress=http://orders-api:80
```

Resolve to `(Uri localAddress, RemoteAddressResolution remoteResolution)` and pass `localAddress` and `remoteResolution.Addresses` into the bus configuration.

### 3. Register the bus and transport

In `Program.cs` (or your composition root):

```csharp
using Transponder;
using Transponder.Transports.Grpc;

// After resolving local and remote addresses (e.g. from TransponderSettings):
builder.Services.AddTransponder(localAddress, options =>
{
    options.TransportBuilder.UseGrpc(localAddress, remoteResolution.Addresses);
    options.RequestAddressResolver = TransponderRequestAddressResolver.Create(
        remoteResolution.Addresses,
        remoteResolution.Strategy,
        options.RequestPathPrefix,
        options.RequestPathFormatter);
});
```

### 4. Add persistence (optional)

For outbox, saga state, or persisted message scheduling:

```csharp
// PostgreSQL persistence (outbox, inbox, saga, scheduled messages)
builder.Services.AddTransponderPostgreSqlPersistence();

// Redis cache (e.g. for coordination)
builder.Services.AddTransponderRedisCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Redis");
});
```

### 5. Configure outbox (optional)

```csharp
options.UseOutbox(outbox =>
{
    outbox.DeadLetterAddress = dlqAddress;  // optional
});
```

### 6. Configure saga (optional)

```csharp
options.UseSagaChoreography(registration =>
{
    registration.AddSaga<CreateBookingSaga, CreateBookingSagaState>(endpoint =>
    {
        var inputAddress = TransponderRequestAddressResolver.Create(localAddress)(typeof(CreateBookingRequest))
            ?? throw new InvalidOperationException("Failed to resolve input address.");
        endpoint.StartWith<CreateBookingRequest>(inputAddress);
    });
});
```

### 7. Host the bus and transport endpoints

- Start the bus (e.g. `TransponderBusHostedService`).
- In ASP.NET Core, register gRPC services and map the transport endpoint:

```csharp
builder.Services.AddGrpc(options => options.Interceptors.Add<GrpcTransportServerInterceptor>());
// ...
app.MapGrpcService<GrpcTransportService>();
```

---

## Configuration Reference

| Area | Source | Description |
|------|--------|-------------|
| **Local address** | `TransponderSettings:LocalBaseAddress` or `TransponderDefaults__LocalAddress` | This service’s base URL for the transport (e.g. `http://bookings-api:80`). |
| **Remote address(es)** | `TransponderSettings:RemoteBaseAddress` or `TransponderDefaults__RemoteAddress` | Other service(s) for request/response. In K8s use service DNS. |
| **Request timeout** | `TransponderBusOptions.DefaultRequestTimeout` | Default timeout for request/response (e.g. 30s). |
| **Outbox** | `UseOutbox(...)` | Enables transactional outbox; optional dead-letter address. |
| **Scheduler** | `UsePersistedMessageScheduler(...)` | Persisted message scheduling; optional dead-letter. |
| **Connection strings** | `ConnectionStrings:Transponder`, `ConnectionStrings:Redis` | Used by PostgreSQL persistence and Redis. |

---

## Applying Configuration in Different Environments

- **Local**: Use `LocalBaseAddress` / `RemoteBaseAddress` in appsettings (e.g. `http://localhost:5187`, `http://localhost:5296`).
- **Kubernetes**: Use `TransponderDefaults__LocalAddress` and `TransponderDefaults__RemoteAddress` with Service names (e.g. `http://sol9-bookings-bookings-api:80`, `http://sol9-orders-orders-api:80`).
- **Docker Compose**: Same as K8s but with service names from the compose file (e.g. `http://bookings-api:80`).

---

## Usage Examples

**Send a request (request/response):**

```csharp
var client = _clientFactory.CreateRequestClient<CreateBookingRequest>();
var response = await client.GetResponseAsync<CreateBookingResponse>(
    new CreateBookingRequest(orderId, customerName));
```

**Publish an event:**

```csharp
await _publishEndpoint.PublishAsync(new OrderCreatedIntegrationEvent(order.Id));
```

**Send (fire-and-forget):**

```csharp
await _sendEndpoint.SendAsync(destinationAddress, new CreateBookingRequest(...));
```

---

## See Also

- [Transponder Transports](Transports/README.md) – gRPC, SignalR, Kafka, RabbitMQ, etc.
- [Deployment Guide](../Deployment/README.md) – gRPC and Transponder service configuration in Kubernetes.
