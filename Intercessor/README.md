# Intercessor: A Robust Intercessor Pattern Implementation with Built-in Resilience

Intercessor is a .NET library that provides a powerful Intercessor pattern implementation with built-in resilience, validation, caching, and monitoring capabilities. It simplifies application architecture by decoupling request handlers from their senders while providing robust middleware behaviors for cross-cutting concerns.

The library implements the Intercessor pattern through a pipeline-based architecture that supports commands, queries, and notifications. Each request flows through configurable behaviors including validation, retry policies, circuit breakers, caching, and logging. This design promotes clean separation of concerns while providing enterprise-grade reliability features out of the box.

## Repository Structure
```
Intercessor/
├── Abstractions/                   # Core interfaces defining the Intercessor pattern contracts
│   ├── ICommand.cs                 # Command interfaces for state-changing operations
│   ├── IQuery.cs                   # Query interfaces for data retrieval operations
│   ├── INotification.cs            # Notification interfaces for event handling
│   └── IRequestHandler.cs          # Base handler interfaces for processing requests
├── Behaviours/                     # Pipeline behaviors implementing cross-cutting concerns
│   ├── CircuitBreaker.cs           # Circuit breaker pattern for fault tolerance
│   ├── RedisCaching.cs             # Redis-based caching for query results
│   ├── RetryBehavior.cs            # Exponential backoff retry logic
│   └── Validation.cs               # FluentValidation request validation
├── Generators/                     # Source generators for handler implementations
│   ├── CommandHandler.cs           # Generates command handler implementations
│   ├── QueryHandler.cs             # Generates query handler implementations
│   └── NotificationHandler.cs      # Generates notification handler implementations
└── Core/                           # Core implementation classes
    ├── Sender.cs                   # Main request dispatcher
    ├── Publisher.cs                # Notification publisher
    └── IntercessorBuilder.cs       # Fluent builder for configuration
```

## Usage Instructions
### Prerequisites
- .NET 8.0 SDK or later
- Redis server (for caching support)
- NuGet package manager

### Installation

```bash
# Using .NET CLI
dotnet add package Intercessor

# Using Package Manager Console
Install-Package Intercessor
```

### Quick Start

1. Register Intercessor in your dependency injection container:

```csharp
services.AddIntercessor(builder => {
    builder.AddAssembly(typeof(Program).Assembly);
});
```

2. Define a query:

```csharp
public class GetUserQuery : IQuery<UserDto>
{
    public int UserId { get; set; }
}

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

3. Use the Intercessor:

```csharp
public class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> GetUser(int id)
    {
        var query = new GetUserQuery { UserId = id };
        var result = await _sender.SendAsync(query);
        return Ok(result);
    }
}
```

### More Detailed Examples

Using validation behavior:

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
    }
}
```

Using caching behavior:

```csharp
public class CachedUserQuery : IQuery<UserDto>, ICachedQuery<UserDto>
{
    public int UserId { get; set; }
    public string CacheKey => $"user:{UserId}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
```

### Troubleshooting

Common issues and solutions:

1. Handler Not Found Exception
```
Error: No handler registered for GetUserQuery
Solution: Ensure the assembly containing the handler is registered with AddIntercessor()
```

2. Redis Connection Issues
```csharp
// Check Redis connection in startup
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "IntercessorCache:";
});
```

3. Validation Failures
- Enable debug logging to see validation details
- Check validator registration in DI container
- Verify validation rules in validator classes

## Data Flow

Intercessor processes requests through a pipeline of behaviors before reaching the handler. Each behavior can modify, validate, or intercept the request/response.

```ascii
Request → [Validation] → [Circuit Breaker] → [Retry] → [Cache Check] → Handler → [Cache Update] → Response
```

Component interactions:
- Sender dispatches requests to appropriate handlers
- Behaviors are executed in reverse registration order
- Validation occurs before other behaviors
- Circuit breaker prevents cascading failures
- Redis cache checks/updates happen around handler execution
- Logging captures cross-cutting telemetry
- Retry policy handles transient failures