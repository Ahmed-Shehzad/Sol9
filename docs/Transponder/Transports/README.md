# Transponder Transports

Transponder supports multiple transport implementations, each optimized for different use cases and scenarios.

## Available Transports

| Transport | Use Case | Best For |
|-----------|----------|----------|
| **gRPC** | Service-to-service RPC | High-performance inter-service communication |
| **SignalR** | Real-time web | Bidirectional web client communication |
| **SSE** | One-way real-time | Dashboards, live feeds, notifications |
| **Webhooks** | HTTP callbacks | External integrations, webhook delivery |
| **Kafka** | Event streaming | High-throughput event processing |
| **RabbitMQ** | Message broker | Complex routing, queues, exchanges |
| **AWS SQS/SNS** | Cloud messaging | AWS-native messaging |
| **Azure Service Bus** | Cloud messaging | Azure-native messaging |

## Quick Reference

### gRPC Transport

**When to use**: Service-to-service communication, request/response patterns, high performance requirements.

```csharp
options.TransportBuilder.UseGrpc(
    localAddress: new Uri("http://localhost:5187"),
    remoteAddresses: new[] { new Uri("http://localhost:5296") });
```

[Full Documentation](gRPC.md)

### SignalR Transport

**When to use**: Real-time web applications, bidirectional communication, live updates.

```csharp
options.TransportBuilder.UseSignalR(
    localAddress: new Uri("http://localhost:5187"),
    remoteAddresses: new[] { new Uri("http://localhost:5296") });
```

[Full Documentation](SignalR.md)

### SSE Transport

**When to use**: One-way real-time updates, dashboards, live feeds, browser-based clients.

```csharp
options.TransportBuilder.UseSse(localAddress: new Uri("http://localhost:5187"));
```

[Full Documentation](SSE.md)

### Webhooks Transport

**When to use**: HTTP-based integrations, external system callbacks, webhook delivery.

```csharp
options.TransportBuilder.UseWebhooks(
    localAddress: new Uri("http://localhost:5187"),
    options: webhookOptions => { /* configure */ });
```

[Full Documentation](Webhooks.md)

### Kafka Transport

**When to use**: High-throughput event streaming, event sourcing, large-scale event processing.

```csharp
options.TransportBuilder.UseKafka(
    bootstrapServers: "localhost:9092",
    options: kafkaOptions => { /* configure */ });
```

[Full Documentation](Kafka.md)

### RabbitMQ Transport

**When to use**: Complex routing scenarios, queues, exchanges, traditional message broker patterns.

```csharp
options.TransportBuilder.UseRabbitMq(
    connectionString: "amqp://localhost:5672",
    options: rabbitOptions => { /* configure */ });
```

[Full Documentation](RabbitMQ.md)

### AWS Transport

**When to use**: AWS-native applications, cloud messaging, SQS/SNS integration.

```csharp
options.TransportBuilder.UseAws(
    region: "us-east-1",
    options: awsOptions => { /* configure */ });
```

[Full Documentation](AWS.md)

### Azure Service Bus Transport

**When to use**: Azure-native applications, cloud messaging, Azure Service Bus integration.

```csharp
options.TransportBuilder.UseAzureServiceBus(
    connectionString: "Endpoint=sb://...",
    options: azureOptions => { /* configure */ });
```

[Full Documentation](AzureServiceBus.md)

## Choosing a Transport

### Performance Requirements

- **gRPC**: Highest performance, binary protocol, HTTP/2
- **Kafka**: Highest throughput for event streaming
- **RabbitMQ**: Good performance with advanced routing
- **SignalR/SSE**: Optimized for web clients

### Use Case

- **Service-to-Service**: gRPC (recommended)
- **Web Clients**: SignalR or SSE
- **Event Streaming**: Kafka
- **Cloud Integration**: AWS or Azure transports
- **External Webhooks**: Webhooks transport

### Network Environment

- **Internal Services**: gRPC
- **Internet-Facing**: SignalR, SSE, Webhooks
- **Cloud**: AWS or Azure transports
- **On-Premises**: RabbitMQ, Kafka

## Transport Features Comparison

| Feature | gRPC | SignalR | SSE | Webhooks | Kafka | RabbitMQ |
|---------|------|---------|-----|----------|-------|----------|
| Request/Response | ✅ | ❌ | ❌ | ✅ | ❌ | ✅ |
| Publish/Subscribe | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ |
| Bidirectional | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ |
| Durable | ✅* | ❌ | ❌ | ❌ | ✅ | ✅ |
| High Throughput | ✅ | ✅ | ✅ | ⚠️ | ✅ | ✅ |
| Web Browser | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ |

*With outbox pattern

## Configuration Examples

### Multiple Transports

You can configure multiple transports for different use cases:

```csharp
builder.Services.AddTransponder(localAddress, options =>
{
    // gRPC for service-to-service
    options.TransportBuilder.UseGrpc(localAddress, serviceAddresses);
    
    // SignalR for web clients
    options.TransportBuilder.UseSignalR(localAddress, webAddresses);
    
    // Kafka for event streaming
    options.TransportBuilder.UseKafka("localhost:9092");
});
```

### Transport-Specific Resilience

Configure resilience per transport:

```csharp
var grpcSettings = new GrpcHostSettings(
    localAddress,
    resilienceOptions: new TransportResilienceOptions
    {
        EnableRetry = true,
        EnableCircuitBreaker = true,
        Retry = new TransportRetryOptions { MaxRetryAttempts = 3 }
    });

options.TransportBuilder.UseGrpc(grpcSettings);
```

## See Also

- [Transponder Core Documentation](../README.md)
- Individual transport documentation linked above
