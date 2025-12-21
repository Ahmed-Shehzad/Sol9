# Transponder
Core bus implementation and scheduling for the Transponder messaging stack.

## Purpose
- Provide `IBus`/`IBusControl` implementations for send, publish, request, and scheduling.
- Offer in-memory and persisted message scheduling.
- Provide opt-in saga orchestration and choreography support.

## Key types
- `TransponderBus`
- `TransponderBusOptions`
- `IMessageScheduler`, `InMemoryMessageScheduler`, `PersistedMessageScheduler`
- `SagaRegistrationBuilder`, `SagaConsumeContext<TState, TMessage>`
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

services.UseSagaOrchestration(sagas =>
{
    sagas.AddSaga<MySaga, MySagaState>(cfg =>
    {
        cfg.StartWith<StartOrder>(new Uri("transponder://local/start-order"));
        cfg.Handle<OrderCompleted>(new Uri("transponder://local/order-completed"));
    });
});
```
