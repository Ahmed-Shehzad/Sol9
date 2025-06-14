# Sol9

## Description
Sol9 is a comprehensive .NET solution that provides robust architectural components for building scalable, resilient, and maintainable microservices and distributed applications. It implements advanced patterns for command/query handling, event-driven architecture, and data persistence with built-in resilience features.

## Features
- **Intercessor Pattern**: Decouples request handlers from senders with built-in middleware pipeline
- **Event-Driven Architecture**: Implements reliable event publishing and consumption across services
- **Resilience Patterns**: Circuit breakers, retries, and caching for robust application behavior
- **Domain-Driven Design**: Core abstractions for entities, aggregates, and domain events
- **Outbox Pattern**: Ensures reliable message delivery with transactional outbox
- **Multiple Transport Options**: Support for RabbitMQ, in-memory, and extensible transport mechanisms

## Components

### Intercessor
A powerful mediator pattern implementation with built-in resilience, validation, caching, and monitoring capabilities. It simplifies application architecture by decoupling request handlers from their senders while providing robust middleware behaviors for cross-cutting concerns.

Key features:
- Pipeline-based architecture for commands, queries, and notifications
- Built-in behaviors: validation, retry policies, circuit breakers, caching, and logging
- Source generators for handler implementations

### Transponder
An event bus implementation that facilitates reliable communication between services using various transport mechanisms. It provides a consistent API for publishing and consuming events across different messaging infrastructures.

Key features:
- Support for multiple transport mechanisms (RabbitMQ, in-memory)
- Transactional outbox pattern for reliable message delivery
- Automatic retry policies and dead-letter handling

### Sol9.Core
Core domain abstractions and utilities for building domain-driven applications:
- Entity and aggregate root base classes
- Auditable and soft-deletable entity interfaces
- Domain event handling infrastructure

## Installation
```bash
# Clone the repository
git clone https://github.com/yourusername/Sol9.git
cd Sol9

# Build the solution
dotnet build
```

## Usage
### Using Intercessor for Command/Query Handling
```csharp
// Define a command
public class CreateUserCommand : ICommand<UserDto>
{
    public string Username { get; set; }
    public string Email { get; set; }
}

// Implement a handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Implementation
        return new UserDto { /* ... */ };
    }
}

// Use the sender to dispatch commands
public class UserService
{
    private readonly ISender _sender;

    public UserService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<UserDto> CreateUser(string username, string email)
    {
        var command = new CreateUserCommand { Username = username, Email = email };
        return await _sender.SendAsync(command);
    }
}
```

### Using Transponder for Event Publishing
```csharp
// Define an event
public class UserCreatedEvent : IIntegrationEvent
{
    public string UserId { get; set; }
    public string Username { get; set; }
}

// Publish an event
public class UserService
{
    private readonly IBusPublisher _publisher;

    public UserService(IBusPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task NotifyUserCreated(string userId, string username)
    {
        var @event = new UserCreatedEvent { UserId = userId, Username = username };
        await _publisher.PublishAsync(@event);
    }
}
```

## Project Structure
```
Sol9/
├── Intercessor/            # Mediator pattern implementation
├── Transponder/            # Event bus core implementation
├── Transponder.RabbitMQ/   # RabbitMQ transport for event bus
├── Transponder.Core/       # Core abstractions for event handling
├── Transponder.Storage/    # Storage implementations for outbox pattern
├── Sol9.Core/              # Domain-driven design core abstractions
└── TestApi/                # Example API implementation
```

## Technologies Used
- .NET 8.0
- RabbitMQ
- Entity Framework Core
- Redis (for caching)
- FluentValidation
- Polly (for resilience policies)

## License
This project is licensed under the terms of the license included in the repository.

## Contact
Your Name - [your.email@example.com](mailto:your.email@example.com)

Project Link: [https://github.com/yourusername/Sol9](https://github.com/yourusername/Sol9)