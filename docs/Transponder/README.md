# Transponder Messaging Framework

Transponder is an enterprise-grade messaging framework for .NET that provides reliable, transport-agnostic messaging patterns with support for multiple transport implementations.

## Overview

Transponder abstracts messaging patterns from transport implementations, allowing you to:
- Switch transports without changing business logic
- Use consistent messaging patterns across different transports
- Implement reliable messaging with outbox/inbox patterns
- Orchestrate distributed transactions with sagas
- Schedule and delay message delivery

## Core Concepts

### Messages

All messages in Transponder implement `IMessage`:

```csharp
public sealed record CreateBookingRequest(Ulid OrderId, string CustomerName) : IMessage;
```

For correlation tracking, implement `ICorrelatedMessage`:

```csharp
public sealed record OrderCreatedEvent(Ulid OrderId) : ICorrelatedMessage
{
    public Ulid CorrelationId { get; init; } = Ulid.NewUlid();
}
```

### Bus

The `IBus` interface provides the main entry point for messaging operations:

- **Publish**: Broadcast messages to all subscribers
- **Send**: Send messages to specific destinations
- **Request/Response**: Synchronous communication between services
- **Schedule**: Delay or schedule message delivery

### Transports

Transponder supports multiple transport implementations:
- gRPC (recommended for service-to-service)
- SignalR (real-time web clients)
- SSE (one-way real-time updates)
- Webhooks (HTTP callbacks)
- Kafka (event streaming)
- RabbitMQ (message broker)
- AWS SQS/SNS (cloud messaging)
- Azure Service Bus (cloud messaging)

## Getting Started

### 1. Install Transponder

```bash
dotnet add package Transponder
dotnet add package Transponder.Transports.Grpc  # or your preferred transport
```

### 2. Register Services

In your `Program.cs`:

```csharp
using Transponder;
using Transponder.Transports.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Register Transponder with gRPC transport
builder.Services.AddTransponder(
    new Uri("http://localhost:5187"),
    options =>
    {
        options.TransportBuilder.UseGrpc(
            localAddress: new Uri("http://localhost:5187"),
            remoteAddresses: new[] { new Uri("http://localhost:5296") });
        
        // Enable outbox pattern for reliable messaging
        options.UseOutbox();
    });

// Map gRPC service
var app = builder.Build();
app.MapGrpcService<GrpcTransportService>();
```

### 3. Use Messaging

#### Request/Response

```csharp
public class CreateOrderCommandHandler
{
    private readonly IClientFactory _clientFactory;

    public CreateOrderCommandHandler(IClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        // Create request client
        var client = _clientFactory.CreateRequestClient<CreateBookingRequest>();
        
        // Send request and await response
        var response = await client.GetResponseAsync<CreateBookingResponse>(
            new CreateBookingRequest(command.OrderId, command.CustomerName));
        
        return new OrderDto { BookingId = response.BookingId };
    }
}
```

#### Publish Events

```csharp
public class OrderService
{
    private readonly IPublishEndpoint _publisher;

    public OrderService(IPublishEndpoint publisher)
    {
        _publisher = publisher;
    }

    public async Task CreateOrderAsync(Order order)
    {
        // Persist order
        await _repository.SaveAsync(order);
        
        // Publish event
        await _publisher.PublishAsync(new OrderCreatedEvent(order.Id));
    }
}
```

#### Send Messages

```csharp
public class NotificationService
{
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public NotificationService(ISendEndpointProvider sendEndpointProvider)
    {
        _sendEndpointProvider = sendEndpointProvider;
    }

    public async Task SendNotificationAsync(string userId, NotificationMessage message)
    {
        var endpoint = await _sendEndpointProvider.GetSendEndpointAsync(
            new Uri($"signalr://users/{userId}"));
        
        await endpoint.SendAsync(message);
    }
}
```

## When to Use

### Use Request/Response When:
- You need synchronous communication between services
- You need a response to continue processing
- You're calling a remote procedure (RPC pattern)
- Example: Creating a booking from an order

### Use Publish/Subscribe When:
- Multiple services need to react to an event
- You want to decouple producers from consumers
- Events are fire-and-forget
- Example: OrderCreated event notifying multiple services

### Use Send When:
- You need to target a specific destination
- You want point-to-point messaging
- You need to control message routing
- Example: Sending a notification to a specific user

### Use Sagas When:
- You need to coordinate multiple steps across services
- You need distributed transaction orchestration
- You need compensation logic for failures
- Example: Order processing with payment and inventory

### Use Outbox Pattern When:
- You need guaranteed message delivery
- Messages must be sent as part of a database transaction
- You need to prevent message loss on failures
- Example: Publishing events after saving to database

## Where to Use

### API Layer (Program.cs)
- Register Transponder services
- Configure transports
- Map transport endpoints (gRPC, SignalR hubs, etc.)

```csharp
// Bookings.API/Program.cs
builder.Services.AddTransponder(localAddress, options =>
{
    options.TransportBuilder.UseGrpc(localAddress, remoteAddresses);
    options.UseOutbox();
});
```

### Application Layer
- Use `IRequestClient<T>` for request/response
- Use `IPublishEndpoint` for events
- Use `ISendEndpointProvider` for targeted sends
- Implement saga handlers

```csharp
// Bookings.Application/Handlers/CreateBookingHandler.cs
public class CreateBookingHandler
{
    private readonly IRequestClient<ValidateOrderRequest> _client;
    
    public async Task HandleAsync(CreateBookingCommand command)
    {
        var response = await _client.GetResponseAsync<ValidateOrderResponse>(
            new ValidateOrderRequest(command.OrderId));
        // ...
    }
}
```

### Infrastructure Layer
- Implement persistence for outbox/inbox
- Implement saga state repositories
- Configure database contexts

## Use Cases

### 1. Service-to-Service Communication

**Scenario**: Orders service needs to create a booking in Bookings service.

```csharp
// Orders.Application
public class CreateOrderHandler
{
    private readonly IClientFactory _clientFactory;
    
    public async Task HandleAsync(CreateOrderCommand command)
    {
        var client = _clientFactory.CreateRequestClient<CreateBookingRequest>();
        var response = await client.GetResponseAsync<CreateBookingResponse>(
            new CreateBookingRequest(command.OrderId, command.CustomerName));
        
        // Use response.BookingId
    }
}
```

### 2. Event-Driven Architecture

**Scenario**: When an order is created, notify multiple services.

```csharp
// Orders.Application
public class CreateOrderHandler
{
    private readonly IPublishEndpoint _publisher;
    
    public async Task HandleAsync(CreateOrderCommand command)
    {
        var order = Order.Create(command.CustomerName, command.TotalAmount);
        await _repository.SaveAsync(order);
        
        // Publish event - multiple handlers can react
        await _publisher.PublishAsync(new OrderCreatedEvent(order.Id));
    }
}
```

### 3. Saga Orchestration

**Scenario**: Process order with payment and inventory in a distributed transaction.

```csharp
// Orders.Application/Sagas/ProcessOrderSaga.cs
public class ProcessOrderSagaState : ISagaState
{
    public Ulid CorrelationId { get; set; }
    public Ulid? ConversationId { get; set; }
    public int Version { get; set; }
    public OrderStatus Status { get; set; }
}

public class ProcessOrderSagaHandler : ISagaMessageHandler<ProcessOrderSagaState, OrderCreatedEvent>
{
    public async Task HandleAsync(ISagaConsumeContext<ProcessOrderSagaState, OrderCreatedEvent> context)
    {
        var state = context.State;
        
        // Step 1: Charge payment
        await context.Bus.SendAsync(new ChargePaymentCommand(state.OrderId));
        
        // Step 2: Reserve inventory
        await context.Bus.SendAsync(new ReserveInventoryCommand(state.OrderId));
        
        state.Status = OrderStatus.Processing;
        await context.SaveAsync();
    }
}
```

### 4. Reliable Messaging with Outbox

**Scenario**: Ensure events are published even if service crashes.

```csharp
// Configure outbox
builder.Services.AddTransponder(localAddress, options =>
{
    options.UseOutbox(opt =>
    {
        opt.BatchSize = 100;
        opt.PollInterval = TimeSpan.FromSeconds(5);
    });
});

// In your handler - message is stored in outbox transactionally
public class CreateOrderHandler
{
    private readonly IBus _bus;
    private readonly IStorageSession _session;
    
    public async Task HandleAsync(CreateOrderCommand command)
    {
        using var session = await _sessionFactory.CreateSessionAsync();
        
        var order = Order.Create(command.CustomerName, command.TotalAmount);
        session.Orders.Add(order);
        
        // This is stored in outbox, not sent immediately
        await _bus.PublishAsync(new OrderCreatedEvent(order.Id));
        
        // Both order and message are committed together
        await session.CommitAsync();
    }
}
```

### 5. Scheduled Messages

**Scenario**: Send reminder email 24 hours after order creation.

```csharp
public class CreateOrderHandler
{
    private readonly IMessageScheduler _scheduler;
    
    public async Task HandleAsync(CreateOrderCommand command)
    {
        var order = Order.Create(command.CustomerName, command.TotalAmount);
        await _repository.SaveAsync(order);
        
        // Schedule reminder for 24 hours later
        await _scheduler.SchedulePublishAsync(
            new SendReminderEmailEvent(order.Id),
            TimeSpan.FromHours(24));
    }
}
```

## Configuration

### Transport Configuration

Each transport has specific configuration options. See transport-specific documentation:
- [gRPC Transport](Transports/gRPC.md)
- [SignalR Transport](Transports/SignalR.md)
- [SSE Transport](Transports/SSE.md)
- [Kafka Transport](Transports/Kafka.md)
- [RabbitMQ Transport](Transports/RabbitMQ.md)

### Outbox Configuration

```csharp
options.UseOutbox(opt =>
{
    opt.BatchSize = 100;                    // Messages per batch
    opt.PollInterval = TimeSpan.FromSeconds(5);  // Poll frequency
    opt.RetryDelay = TimeSpan.FromSeconds(10);   // Retry delay
    opt.MaxConcurrentDestinations = 5;      // Parallel dispatch
    opt.DeadLetterAddress = new Uri("dlq://failed");  // DLQ for failures
});
```

### Message Scheduler Configuration

```csharp
options.UsePersistedMessageScheduler(opt =>
{
    opt.PollInterval = TimeSpan.FromSeconds(1);
    opt.BatchSize = 50;
    opt.DeadLetterAddress = new Uri("dlq://failed");
});
```

## Persistence

Transponder supports multiple persistence backends:

- **Entity Framework Core**: PostgreSQL, SQL Server
- **Redis**: For saga state and caching
- **In-Memory**: For testing

### PostgreSQL Persistence

```csharp
builder.Services.AddTransponderPersistencePostgreSql(options =>
{
    options.ConnectionString = "Host=localhost;Database=transponder;...";
    options.Schema = "transponder";
});
```

## Best Practices

1. **Always use outbox for cross-service messaging** to ensure reliability
2. **Make message handlers idempotent** to handle duplicates
3. **Use correlation IDs** for request tracing
4. **Version your message contracts** for backward compatibility
5. **Use HTTPS for gRPC** to enable HTTP/2
6. **Configure dead-letter queues** for unprocessable messages
7. **Monitor saga state** for stuck or failed sagas
8. **Use structured logging** with correlation IDs

## Advanced Features

### Custom Message Scopes

Implement `ITransponderMessageScopeProvider` for custom context propagation:

```csharp
public class CustomScopeProvider : ITransponderMessageScopeProvider
{
    public IDisposable BeginScope(ITransportMessage message)
    {
        // Create custom scope (e.g., tenant context)
        return new CustomScope(message);
    }
}
```

### Custom Serialization

Implement `IMessageSerializer` for custom serialization:

```csharp
public class CustomSerializer : IMessageSerializer
{
    public Task<ReadOnlyMemory<byte>> SerializeAsync(object message, Type type, CancellationToken cancellationToken = default)
    {
        // Custom serialization logic
    }
    
    public Task<object> DeserializeAsync(ReadOnlyMemory<byte> data, Type type, CancellationToken cancellationToken = default)
    {
        // Custom deserialization logic
    }
}
```

## Troubleshooting

### Messages Not Being Delivered

1. Check transport connectivity
2. Verify outbox dispatcher is running
3. Check dead-letter queue for failures
4. Review logs for errors

### Saga Stuck

1. Check saga state in database
2. Verify all saga handlers are registered
3. Check for exceptions in saga handlers
4. Review correlation ID matching

### Performance Issues

1. Increase outbox batch size
2. Adjust poll intervals
3. Use connection pooling
4. Monitor transport-specific metrics

## See Also

- [Transport Documentation](Transports/README.md)
- [Persistence Documentation](../Transponder.Persistence/README.md)
- [Saga Patterns Guide](Sagas.md)
