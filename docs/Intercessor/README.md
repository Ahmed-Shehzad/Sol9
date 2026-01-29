# Intercessor

Intercessor is the **mediator-style library** used in Sol9 for in-process **commands**, **queries**, and **notifications**. It dispatches requests to a single handler (or multiple handlers for notifications) and supports **pipeline behaviors** (validation, logging, retry, caching, etc.).

---

## When to Use

- **CQRS-style application layer**: Commands and queries are sent via `ISender`; handlers live in the application layer and stay decoupled from controllers.
- **Cross-cutting concerns**: Add logging, validation, retries, or caching around every request without touching handlers.
- **In-process only**: All handlers run in the same process; for cross-service or cross-process messaging use **Transponder** instead.
- **Notifications**: Publish an event to multiple handlers with `IPublisher.PublishAsync` (e.g. “order created” → email, analytics, cache invalidation).

---

## Where to Use

- **Application layer**: Define commands, queries, handlers, and validators; register Intercessor and point it at the application assembly.
- **API layer**: Controllers (or minimal APIs) resolve `ISender` / `IPublisher` and send commands/queries or publish notifications; no direct dependency on handlers.
- **Not for cross-service calls**: Use Transponder for Orders → Bookings; use Intercessor inside each service for “controller → command handler” and “event → notification handlers”.

---

## Why to Use

- **Decoupling**: Controllers depend only on `ISender`/`IPublisher`; handlers can be added or changed without changing the API surface.
- **Testability**: Handlers are easy to unit test; pipeline behaviors can be tested in isolation.
- **Consistent pipeline**: Validation, logging, retry, and caching apply to all (or selected) requests via behaviors.
- **Integration with Verifier**: ValidationBehavior runs Verifier validators before the handler; failed validation throws `ValidationException`.

---

## Why Not to Use

- **Cross-process or cross-service**: Use **Transponder** for messaging between services or processes; Intercessor is in-process only.
- **Very simple CRUD with no cross-cutting needs**: If you have one handler and no validation/logging pipeline, you can call the handler directly; Intercessor still helps keep controllers thin and consistent.
- **High-frequency, latency-critical paths**: The pipeline adds indirection; for hot paths that need minimal overhead, measure and consider direct calls if necessary.

---

## How to Set Up

### 1. Add the package

```bash
dotnet add package Intercessor
```

Verifier is used for validation behavior; add it if you use validators:

```bash
dotnet add package Verifier
```

### 2. Register Intercessor

Register Intercessor and specify which assembly (or assemblies) contain handlers and validators:

```csharp
using Intercessor;

builder.Services.AddIntercessor(options =>
{
    options.RegisterFromAssembly(typeof(BookingsApplication).Assembly);
    // options.RegisterFromAssembly(Assembly.GetExecutingAssembly());
});
```

This registers:

- `ISender`, `IPublisher`
- All `IQueryHandler<TQuery, TResponse>`, `ICommandHandler<TCommand>`, `ICommandHandler<TCommand, TResponse>`, `INotificationHandler<TNotification>`
- All `IPipelineBehavior<TRequest, TResponse>` and `IPipelineBehavior<TRequest>` (from the same assemblies)
- Verifier validators from the same assemblies
- Built-in `ValidationBehavior` for requests

### 3. (Optional) Add custom pipeline behaviors

```csharp
options.AddBehavior<LoggingBehavior<,>>();
options.AddBehavior<RetryBehavior<,>>();
options.AddBehavior<RedisCachingBehavior<,>>();  // for cacheable queries
```

Behaviors run in registration order (last registered = outermost). ValidationBehavior is registered by default.

---

## Configuration

Intercessor has no appsettings section. Configuration is at registration:

| Method | Description |
|--------|-------------|
| `RegisterFromAssembly(Assembly)` | Scan assembly for handlers and behaviors; register them and wire Verifier. |
| `AddBehavior<TBehavior>()` | Register a pipeline behavior type (e.g. logging, retry, caching). |

Behavior-specific config (e.g. Redis connection for `RedisCachingBehavior`) is done via normal DI (options types, connection strings).

---

## Applying Configuration

- **Single application assembly**: `RegisterFromAssembly(typeof(YourApplication).Assembly)`.
- **Multiple assemblies**: Call `RegisterFromAssembly` for each assembly that contains handlers or validators.
- **Behaviors**: Add custom behaviors with `AddBehavior<>`; order matters (first added = runs first, i.e. closest to the handler).

---

## Request Types and Handlers

| Contract | Handler | Use case |
|----------|---------|----------|
| `IRequest<TResponse>` | `IRequestHandler<TRequest, TResponse>` | Request with a response (query or command that returns a value). |
| `IRequest` | `IRequestHandler<TRequest>` | Request with no response (void command). |
| `ICommand<TResponse>` | `ICommandHandler<TCommand, TResponse>` | Command that returns a value (e.g. create order → order id). |
| `ICommand` | `ICommandHandler<TCommand>` | Command with no return value. |
| `IQuery<TResponse>` | `IQueryHandler<TQuery, TResponse>` | Query that returns data. |
| `INotification` | `INotificationHandler<TNotification>` | Event; multiple handlers can be invoked. |

---

## Usage Examples

**Send a command (with response):**

```csharp
var orderId = await _sender.SendAsync(new CreateOrderCommand("Customer Name", 99.99m));
```

**Send a query:**

```csharp
var bookings = await _sender.SendAsync(new GetBookingsQuery(page, pageSize));
```

**Publish a notification (multiple handlers):**

```csharp
await _publisher.PublishAsync(new OrderCreatedIntegrationEvent(order.Id));
```

**Define a command and handler:**

```csharp
public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) : ICommand<Guid>;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        // ...
        return order.Id.ToGuid();
    }
}
```

**Define a validator (runs automatically via ValidationBehavior):**

```csharp
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.CustomerName).NotEmpty();
        RuleFor(c => c.TotalAmount).Must(a => a > 0, "TotalAmount must be greater than zero.");
    }
}
```

---

## Pipeline Behaviors

Behaviors wrap handler execution. Order: first registered runs first (outermost). Default registration includes:

- **ValidationBehavior**: Runs all `IValidator<TRequest>`; throws `ValidationException` on failure.

You can add:

- **LoggingBehavior**: Log request and response or duration.
- **RetryBehavior**: Retry on transient failures.
- **CircuitBreakerBehavior**: Open circuit after repeated failures.
- **RedisCachingBehavior**: Cache results for requests that implement `ICachedQuery`.

Implement `IPipelineBehavior<TRequest, TResponse>` or `IPipelineBehavior<TRequest>` and register with `AddBehavior<>`.

---

## See Also

- [Verifier](../Verifier/README.md) – Validation framework used by Intercessor’s ValidationBehavior.
- [Transponder](../Transponder/README.md) – Use for cross-service messaging; use Intercessor inside each service.
