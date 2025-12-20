# Transponder.Transports
Transport abstractions, host registry, and resilience helpers for Transponder.

## Purpose
- Define send, publish, and receive contracts for transport implementations.
- Provide the transport builder and registry to plug in providers.
- Apply resilience policies for transport operations.

## Key types
- `ITransportHost`, `ITransportFactory`
- `ISendTransport`, `IPublishTransport`, `IReceiveEndpoint`, `IReceiveContext`
- `ReceiveEndpointConfiguration`, `ReceiveEndpointFaultSettings`
- `TransportResiliencePipeline`, `ResilientSendTransport`, `ResilientPublishTransport`
- `TransponderTransportBuilder`, `TransportRegistry`

## Usage
```csharp
services.AddTransponderTransports(builder =>
{
    // builder.AddRabbitMqTransport(...);
});
```
