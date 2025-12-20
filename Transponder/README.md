# Transponder
Core bus implementation and scheduling for the Transponder messaging stack.

## Purpose
- Provide `IBus`/`IBusControl` implementations for send, publish, request, and scheduling.
- Offer in-memory and persisted message scheduling.

## Key types
- `TransponderBus`
- `TransponderBusOptions`
- `IMessageScheduler`, `InMemoryMessageScheduler`, `PersistedMessageScheduler`
- `RequestClient<TRequest>`
- `JsonMessageSerializer`
- `TransponderRequestAddressResolver`

## Usage
```csharp
services.AddTransponder(
    new Uri("transponder://local"),
    options =>
    {
        options.UsePersistedMessageScheduler();
    });

services.AddTransponderTransports(builder =>
{
    // builder.AddRabbitMqTransport(...);
});
```
