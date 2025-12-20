# Transponder.Transports.Aws
AWS transport adapter for Transponder.

## Purpose
- Provides host settings, topology, and send/publish/receive wiring for AWS transports.
- Integrates with the Transponder transport registry.

## Key types
- `AwsTransportHost`, `AwsTransportFactory`
- `AwsSendTransport`, `AwsPublishTransport`, `AwsReceiveEndpoint`
- `IAwsTransportHostSettings`, `AwsTransportHostSettings`
- `IAwsTopology`

## Registration
```csharp
services.AddTransponderTransports(builder =>
{
    builder.AddAwsTransport(sp => new AwsTransportHostSettings
    {
        // configure settings
    });
});
```
