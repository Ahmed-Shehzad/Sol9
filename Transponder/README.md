# Transponder

A lightweight, flexible integration event handling system for .NET applications.

## Overview

Transponder provides a simple yet powerful way to implement the publish-subscribe pattern in your .NET applications. It allows for decoupled communication between different parts of your application through integration events.

## Features

- Simple event publishing mechanism
- Dependency injection integration
- Automatic handler discovery and registration
- Asynchronous event handling
- Error handling and logging

## Installation

Add the Transponder project to your solution:

```bash
dotnet add reference path/to/Transponder.csproj
```

## Usage

### Setup

Register Transponder in your application's startup:

```csharp
// In Program.cs or Startup.cs
services.AddTransient<TransponderBuilder>();

// Configure Transponder
var builder = new TransponderBuilder(services);
builder.RegisterFromAssembly(typeof(YourHandler).Assembly);
builder.Build();
```

### Defining Events

Create integration events by implementing the `IIntegrationEvent` interface:

```csharp
public class OrderCreatedEvent : IIntegrationEvent
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    // Additional properties...
}
```

### Creating Event Handlers

Implement the `IIntegrationEventHandler<T>` interface to handle specific events:

```csharp
public class OrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Handle the event
        await ProcessOrderAsync(@event.OrderId, cancellationToken);
    }
}
```

### Publishing Events

Inject `IBusPublisher` and use it to publish events:

```csharp
public class OrderService
{
    private readonly IBusPublisher _busPublisher;
    
    public OrderService(IBusPublisher busPublisher)
    {
        _busPublisher = busPublisher;
    }
    
    public async Task CreateOrderAsync(Order order, CancellationToken cancellationToken)
    {
        // Business logic...
        
        // Publish event
        await _busPublisher.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }
}
```

## Dependencies

- .NET 8.0
- Microsoft.Extensions.Hosting.Abstractions
- Microsoft.Extensions.Http.Polly
- Microsoft.Extensions.Resilience
- Scrutor

## License

[Specify your license here]