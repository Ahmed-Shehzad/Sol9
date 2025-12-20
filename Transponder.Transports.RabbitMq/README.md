# Transponder.Transports.RabbitMq
RabbitMQ transport adapter for Transponder.

## Purpose
- Provides host settings, topology, and send/publish/receive wiring for RabbitMQ.
- Integrates with the Transponder transport registry.

## Key types
- `RabbitMqTransportHost`, `RabbitMqTransportFactory`
- `RabbitMqSendTransport`, `RabbitMqPublishTransport`, `RabbitMqReceiveEndpoint`
- `IRabbitMqHostSettings`, `RabbitMqHostSettings`
- `IRabbitMqTopology`

## Registration
```csharp
services.AddTransponderTransports(builder =>
{
    builder.AddRabbitMqTransport(sp => new RabbitMqHostSettings
    {
        // configure settings
    });
});
```
