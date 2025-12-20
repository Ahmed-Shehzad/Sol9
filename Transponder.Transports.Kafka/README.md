# Transponder.Transports.Kafka
Apache Kafka transport adapter for Transponder.

## Purpose
- Provides host settings, topology, and send/publish/receive wiring for Kafka.
- Integrates with the Transponder transport registry.

## Key types
- `KafkaTransportHost`, `KafkaTransportFactory`
- `KafkaSendTransport`, `KafkaPublishTransport`, `KafkaReceiveEndpoint`
- `IKafkaHostSettings`, `KafkaHostSettings`
- `IKafkaTopology`

## Registration
```csharp
services.AddTransponderTransports(builder =>
{
    builder.AddKafkaTransport(sp => new KafkaHostSettings
    {
        // configure settings
    });
});
```
