# Intercessor

Intercessor is a mediator-style request, command, and notification pipeline library for .NET that provides clean separation of concerns and cross-cutting behavior support.

## Overview

Intercessor implements the Mediator pattern, allowing you to:
- Decouple request senders from handlers
- Add cross-cutting concerns via pipeline behaviors
- Organize application logic with commands, queries, and notifications
- Centralize validation, logging, and error handling

## Core Concepts

### Requests

Intercessor supports three types of requests:

1. **Commands**: Operations that change state (may return a result)
2. **Queries**: Operations that read data (always return a result)
3. **Notifications**: Fire-and-forget events (no return value)

### Handlers

Handlers process requests:

- `ICommandHandler<TCommand, TResult>`: Handles commands with return value
- `ICommandHandler<TCommand>`: Handles commands without return value
- `IQueryHandler<TQuery, TResult>`: Handles queries
- `INotificationHandler<TNotification>`: Handles notifications

### Pipeline Behaviors

Behaviors wrap handler execution for cross-cutting concerns:
- Validation
- Logging
- Retry logic
- Circuit breakers
- Performance monitoring

## Getting Started

### 1. Install Intercessor

```bash
dotnet add package Intercessor
```

### 2. Register Services

In your `Program.cs`:

```csharp
using Intercessor;

builder.Services.AddIntercessor(options =>
{
    // Register handlers from assembly
    options.RegisterFromAssembly(typeof(Program).Assembly);
    
    // Add pipeline behaviors
    options.AddBehavior<LoggingBehavior<,>>();
    options.AddBehavior<ValidationBehavior<,>>();
});
```

### 3. Define Commands and Queries

```csharp
// Commands
public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) 
    : ICommand<OrderDto>;

public sealed record CancelOrderCommand(Ulid OrderId) : ICommand;

// Queries
public sealed record GetOrderQuery(Ulid OrderId) : IQuery<OrderDto>;

public sealed record GetOrdersQuery() : IQuery<IReadOnlyList<OrderDto>>;

// Notifications
public sealed record OrderCreatedNotification(Ulid OrderId) : INotification;
```

### 4. Implement Handlers

```csharp
public sealed class CreateOrderCommandHandler 
    : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrdersRepository _repository;

    public CreateOrderCommandHandler(IOrdersRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto> HandleAsync(
        CreateOrderCommand command, 
        CancellationToken cancellationToken = default)
    {
        var order = Order.Create(command.CustomerName, command.TotalAmount);
        await _repository.SaveAsync(order, cancellationToken);
        
        return new OrderDto 
        { 
            Id = order.Id, 
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount 
        };
    }
}

public sealed class GetOrderQueryHandler 
    : IQueryHandler<GetOrderQuery, OrderDto>
{
    private readonly IOrdersRepository _repository;

    public GetOrderQueryHandler(IOrdersRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto> HandleAsync(
        GetOrderQuery query, 
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);
        
        return new OrderDto 
        { 
            Id = order.Id, 
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount 
        };
    }
}
```

### 5. Use in Controllers

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IPublisher _publisher;

    public OrdersController(ISender sender, IPublisher publisher)
    {
        _sender = sender;
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await _sender.SendAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(
        Ulid id,
        CancellationToken cancellationToken)
    {
        var order = await _sender.SendAsync(new GetOrderQuery(id), cancellationToken);
        return Ok(order);
    }
}
```

## When to Use

### Use Commands When:
- You need to perform an action that changes state
- You want to return a result from the operation
- Example: `CreateOrderCommand`, `UpdateOrderCommand`

### Use Queries When:
- You need to read data
- You always need a return value
- Example: `GetOrderQuery`, `GetOrdersQuery`

### Use Notifications When:
- You need fire-and-forget event handling
- Multiple handlers can process the same notification
- Example: `OrderCreatedNotification`, `OrderCancelledNotification`

### Use Pipeline Behaviors When:
- You need cross-cutting concerns (logging, validation, retry)
- You want to centralize common logic
- You need to add functionality without modifying handlers

## Where to Use

### Application Layer

**Commands and Queries**: Define in Application layer, next to handlers.

```
Orders.Application/
├── Commands/
│   ├── CreateOrder/
│   │   ├── CreateOrderCommand.cs
│   │   ├── CreateOrderCommandHandler.cs
│   │   └── CreateOrderCommandValidator.cs
│   └── CancelOrder/
│       ├── CancelOrderCommand.cs
│       └── CancelOrderCommandHandler.cs
├── Queries/
│   ├── GetOrder/
│   │   ├── GetOrderQuery.cs
│   │   └── GetOrderQueryHandler.cs
│   └── GetOrders/
│       ├── GetOrdersQuery.cs
│       └── GetOrdersQueryHandler.cs
└── Notifications/
    └── OrderCreated/
        └── OrderCreatedNotificationHandler.cs
```

### API Layer

**Controllers**: Use `ISender` and `IPublisher` to dispatch requests.

```csharp
// Orders.API/Controllers/OrdersController.cs
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderCommand command)
    {
        var result = await _sender.SendAsync(command);
        return Ok(result);
    }
}
```

### Infrastructure Layer

**Behaviors**: Implement custom behaviors for cross-cutting concerns.

```csharp
// Orders.Infrastructure/Behaviors/PerformanceBehavior.cs
public class PerformanceBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await next();
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request {RequestType} took {ElapsedMs}ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## Use Cases

### 1. Command Handling

**Scenario**: Create an order with validation and logging.

```csharp
// Command
public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) 
    : ICommand<OrderDto>;

// Handler
public sealed class CreateOrderCommandHandler 
    : ICommandHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        // Business logic here
        var order = Order.Create(command.CustomerName, command.TotalAmount);
        await _repository.SaveAsync(order);
        return MapToDto(order);
    }
}

// Controller
[HttpPost]
public async Task<IActionResult> Create(CreateOrderCommand command)
{
    var order = await _sender.SendAsync(command);
    return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
}
```

### 2. Query Handling

**Scenario**: Retrieve orders with caching.

```csharp
// Query
public sealed record GetOrdersQuery() : IQuery<IReadOnlyList<OrderDto>>;

// Handler
public sealed class GetOrdersQueryHandler 
    : IQueryHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> HandleAsync(GetOrdersQuery query)
    {
        var orders = await _repository.GetAllAsync();
        return orders.Select(MapToDto).ToList();
    }
}

// Controller
[HttpGet]
[ResponseCache(Duration = 60)]
public async Task<IActionResult> GetAll()
{
    var orders = await _sender.SendAsync(new GetOrdersQuery());
    return Ok(orders);
}
```

### 3. Notification Handling

**Scenario**: Send email and update cache when order is created.

```csharp
// Notification
public sealed record OrderCreatedNotification(Ulid OrderId) : INotification;

// Handler 1: Send Email
public sealed class SendOrderConfirmationEmailHandler 
    : INotificationHandler<OrderCreatedNotification>
{
    public async Task HandleAsync(OrderCreatedNotification notification)
    {
        // Send email
        await _emailService.SendConfirmationAsync(notification.OrderId);
    }
}

// Handler 2: Update Cache
public sealed class UpdateOrderCacheHandler 
    : INotificationHandler<OrderCreatedNotification>
{
    public async Task HandleAsync(OrderCreatedNotification notification)
    {
        // Update cache
        await _cache.InvalidateAsync($"order:{notification.OrderId}");
    }
}

// In command handler
public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
{
    var order = Order.Create(command.CustomerName, command.TotalAmount);
    await _repository.SaveAsync(order);
    
    // Publish notification - both handlers will execute
    await _publisher.PublishAsync(new OrderCreatedNotification(order.Id));
    
    return MapToDto(order);
}
```

### 4. Pipeline Behaviors

**Scenario**: Add logging and validation to all requests.

```csharp
// Register behaviors
builder.Services.AddIntercessor(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
    options.AddBehavior<LoggingBehavior<,>>();
    options.AddBehavior<ValidationBehavior<,>>();
});

// Logging Behavior
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        try
        {
            var response = await next();
            _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestType}", typeof(TRequest).Name);
            throw;
        }
    }
}
```

## Built-in Behaviors

Intercessor includes several built-in behaviors:

### ValidationBehavior

Automatically validates requests using Verifier validators:

```csharp
// Validator
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}

// Behavior is automatically registered when Verifier is configured
builder.Services.AddVerifier(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
});
```

### LoggingBehavior

Logs request execution (you can implement custom logging behavior).

### RetryBehavior

Retries failed requests (implement custom retry logic).

### CircuitBreakerBehavior

Implements circuit breaker pattern (implement custom circuit breaker).

## Best Practices

1. **One Handler Per Request**: Each command/query should have exactly one handler
2. **Keep Handlers Thin**: Business logic in handlers, infrastructure in repositories
3. **Use Notifications for Side Effects**: Don't return values from notifications
4. **Validate in Behaviors**: Use ValidationBehavior for consistent validation
5. **Log in Behaviors**: Use LoggingBehavior for consistent logging
6. **Organize by Feature**: Group related commands/queries/handlers together
7. **Use Cancellation Tokens**: Always accept CancellationToken in handlers

## Integration with Verifier

Intercessor automatically integrates with Verifier for validation:

```csharp
// Register both
builder.Services.AddVerifier(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddIntercessor(options =>
{
    options.RegisterFromAssembly(typeof(Program).Assembly);
    // ValidationBehavior is automatically added
});
```

## See Also

- [Verifier Documentation](../Verifier/README.md) - Validation framework
- [Transponder Documentation](../Transponder/README.md) - Messaging framework
