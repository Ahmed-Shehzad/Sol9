# RabbitMQ Event Bus Implementation for .NET Applications

Transponder.RabbitMQ is a robust .NET implementation of an event bus using RabbitMQ as the message broker. It provides a reliable and fault-tolerant messaging system with built-in retry policies, dead-letter queues, and message persistence.

This library implements both publishing and consuming interfaces for handling events through RabbitMQ, making it ideal for distributed systems and microservices architectures. It features automatic queue management, message retry policies, and dead-letter handling to ensure reliable message delivery and processing. The implementation supports asynchronous operations and is built on .NET 8.0, leveraging modern C# features for optimal performance and developer experience.

## Repository Structure
```
.
├── RabbitMqConnectionFactory.cs    # Handles RabbitMQ connection management and configuration
├── RabbitMqEventBus.cs            # Core implementation of the event bus with publish/consume functionality
├── README.md                       # Project documentation
└── Transponder.RabbitMQ.csproj    # Project file with .NET 8.0 configuration and dependencies
```

## Usage Instructions
### Prerequisites
- .NET 8.0 SDK or later
- RabbitMQ Server (3.12 or later recommended)
- Access to a RabbitMQ instance (local or remote)
- The following NuGet packages:
  - RabbitMQ.Client (7.1.2)
  - RabbitMQ.Stream.Client (1.8.13)

### Installation

1. Add the package to your project:
```bash
dotnet add package Transponder.RabbitMQ
```

2. Configure RabbitMQ connection in your `Program.cs` or startup configuration:
```csharp
services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
services.AddSingleton<IPublisher, RabbitMqEventBus>();
services.AddSingleton<IConsumer, RabbitMqEventBus>();
```

### Quick Start

1. Define your event class:
```csharp
public class UserCreatedEvent : INotification
{
    public string UserId { get; set; }
    public string Username { get; set; }
}
```

2. Publishing events:
```csharp
public class EventPublisher
{
    private readonly IPublisher _publisher;

    public EventPublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PublishUserCreatedEvent(string userId, string username)
    {
        var event = new UserCreatedEvent 
        { 
            UserId = userId, 
            Username = username 
        };
        await _publisher.PublishAsync(event);
    }
}
```

3. Consuming events:
```csharp
public class EventConsumer
{
    private readonly IConsumer _consumer;

    public EventConsumer(IConsumer consumer)
    {
        _consumer = consumer;
    }

    public async Task ConsumeUserCreatedEvents(CancellationToken cancellationToken)
    {
        await foreach (var event in _consumer.ConsumeAsync<UserCreatedEvent>(cancellationToken))
        {
            // Process the event
            Console.WriteLine($"User created: {event.Username}");
        }
    }
}
```

### More Detailed Examples

Message Processing with Retry Policy:
```csharp
public class RobustEventConsumer
{
    private readonly IConsumer _consumer;
    private readonly ILogger<RobustEventConsumer> _logger;

    public RobustEventConsumer(IConsumer consumer, ILogger<RobustEventConsumer> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    public async Task ProcessEvents(CancellationToken cancellationToken)
    {
        await foreach (var event in _consumer.ConsumeAsync<UserCreatedEvent>(cancellationToken))
        {
            try
            {
                await ProcessEventWithRetry(event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event after retries");
            }
        }
    }

    private async Task ProcessEventWithRetry(UserCreatedEvent event)
    {
        // The event bus already implements retry policy internally
        // Add your business logic here
    }
}
```

### Troubleshooting

Common Issues:

1. Connection Refused
```
Problem: Cannot connect to RabbitMQ server
Error: "System.Net.Sockets.SocketException: Connection refused"
Solution:
- Verify RabbitMQ is running: `rabbitmqctl status`
- Check connection settings in RabbitMqConnectionFactory.cs
- Ensure firewall allows connection to port 5672
```

2. Authentication Failed
```
Problem: Cannot authenticate with RabbitMQ
Error: "RabbitMQ.Client.Exceptions.AuthenticationFailedException"
Solution:
- Verify username and password in RabbitMqConnectionFactory.cs
- Check user permissions: `rabbitmqctl list_users`
- Ensure user has appropriate virtual host permissions
```

Debug Mode:
```csharp
// Enable debug logging in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Transponder.RabbitMQ": "Debug"
    }
  }
}
```

## Data Flow

The event bus implements a publish-subscribe pattern using RabbitMQ exchanges and queues. Messages flow through main exchanges, with retry and dead-letter exchanges handling failed deliveries.

```ascii
Publisher -> Main Exchange -> Main Queue -> Consumer
                 |
                 v
            Retry Exchange -> Retry Queue
                 |
                 v
        Dead Letter Exchange -> Dead Letter Queue
```

Component Interactions:
1. Publisher serializes events to JSON and sends to main exchange
2. Main queue receives messages with dead-letter configuration
3. Consumer processes messages with automatic retry policy
4. Failed messages route to retry queue with 5-second delay
5. Retry queue returns messages to main exchange after delay
6. Permanently failed messages route to dead-letter queue
7. Messages persist with 10-hour expiration by default