# Transponder.Transports.AzureServiceBus
Azure Service Bus transport adapter for Transponder.

## Purpose
- Provides host settings, topology, and send/publish/receive wiring for Azure Service Bus.
- Integrates with the Transponder transport registry.

## Key types
- `AzureServiceBusTransportHost`, `AzureServiceBusTransportFactory`
- `AzureServiceBusSendTransport`, `AzureServiceBusPublishTransport`, `AzureServiceBusReceiveEndpoint`
- `IAzureServiceBusHostSettings`, `AzureServiceBusHostSettings`
- `IAzureServiceBusTopology`, `AzureServiceBusEntityAddress`

## Registration
```csharp
services.AddTransponderTransports(builder =>
{
    builder.AddAzureServiceBusTransport(sp => new AzureServiceBusHostSettings
    {
        // configure settings
    });
});
```
