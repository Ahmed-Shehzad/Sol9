# Transponder Transports

Transponder supports multiple transport implementations for sending and receiving messages. Choose based on your deployment and latency/throughput needs.

| Transport | Typical use | Package |
|-----------|-------------|---------|
| **gRPC** | Service-to-service, high performance, HTTP/2 | `Transponder.Transports.Grpc` |
| **SignalR** | Browser or .NET clients, real-time | `Transponder.Transports.SignalR` |
| **SSE** | Server-sent events, simple streaming | `Transponder.Transports.SSE` |
| **Webhooks** | HTTP callbacks | `Transponder.Transports.Webhooks` |
| **Kafka** | Event streaming, high throughput | `Transponder.Transports.Kafka` |
| **RabbitMQ** | Message broker, queues | `Transponder.Transports.RabbitMq` |
| **AWS / Azure** | Cloud message buses | `Transponder.Transports.Aws`, `Transponder.Transports.AzureServiceBus` |

## gRPC (recommended for service-to-service)

Register in your bus configuration:

```csharp
options.TransportBuilder.UseGrpc(localAddress, remoteAddresses);
```

Requires ASP.NET Core gRPC server and `GrpcTransportService` mapped. See [Transponder README](../README.md) for full setup and configuration (addresses, K8s service names, outbox, saga).

## When to use which

- **Service-to-service (e.g. Orders ↔ Bookings)**: gRPC.
- **Browser or mobile clients**: SignalR or SSE.
- **Event streaming / high throughput**: Kafka.
- **Queue-based workflows**: RabbitMQ or cloud buses.

Configuration (addresses, connection strings, timeouts) is transport-specific; see each transport’s extension methods and options in the codebase.
