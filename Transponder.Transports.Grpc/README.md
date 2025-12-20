# Transponder.Transports.Grpc
gRPC transport adapter for Transponder.

## Purpose
- Provides host settings, topology, and send/publish/receive wiring over gRPC.
- Includes gRPC service definitions for transport-level messaging.

## Key types
- `GrpcTransportHost`, `GrpcTransportFactory`
- `GrpcSendTransport`, `GrpcPublishTransport`, `GrpcReceiveEndpoint`
- `IGrpcHostSettings`, `GrpcHostSettings`
- `IGrpcTopology`

## Registration
```csharp
services.AddTransponderTransports(builder =>
{
    builder.AddGrpcTransport(sp => new GrpcHostSettings
    {
        // configure settings
    });
});
```
