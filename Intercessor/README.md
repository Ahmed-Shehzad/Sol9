# Intercessor
Mediator-style request, command, and notification pipeline.

## Purpose
- Dispatch commands, queries, and notifications to handlers.
- Provide pipeline behaviors for cross-cutting concerns.

## Key types
- `ISender`, `IPublisher`
- `IQueryHandler<,>`, `ICommandHandler<>`, `INotificationHandler<>`
- `IntercessorBuilder`
- `LoggingBehavior`, `RetryBehavior`, `CircuitBreakerBehavior`, `ValidationBehavior`

## Registration
```csharp
services.AddIntercessor(builder =>
{
    builder.RegisterFromAssembly(typeof(SomeHandler).Assembly);
});
```
