# Transponder.Transports.SignalR

SignalR-based transport for realtime, publish-only events. This transport is designed for UI notifications and ephemeral updates.

## Semantics
- Best-effort delivery; no built-in retries beyond any configured resilience pipeline.
- Non-durable: messages are not stored and are lost if clients are disconnected.
- If you need catch-up, publish to a durable transport and project into SignalR as a separate step.
- No receive endpoints: request/response is not supported.

## Registration (server)
```csharp
services.AddSignalR();

services.AddTransponder(localAddress, options =>
{
    options.TransportBuilder.UseSignalR(localAddress, remoteAddresses);
});
```

Map the hub (default path is `/transponder/transport`):
```csharp
app.MapHub<TransponderSignalRHub>("/transponder/transport");
```

## Targeting
Publish defaults to broadcast. Targets can be selected via headers:
- `SignalR.Broadcast` (bool)
- `SignalR.ConnectionId` or `SignalR.ConnectionIds` (comma/semicolon separated)
- `SignalR.ExcludeConnectionId` or `SignalR.ExcludeConnectionIds` (comma/semicolon separated)
- `SignalR.Group` or `SignalR.Groups` (comma/semicolon separated)
- `SignalR.User` or `SignalR.Users` (comma/semicolon separated)

If no target headers are provided, the message is broadcast to all clients.

Send supports the same headers and can also derive targets from destination addresses:
- `signalr://connections/{id}`
- `signalr://users/{id}`
- `signalr://groups/{name}`
- `signalr://broadcast`

## Catch-up path (late joiners)
SignalR does not store messages. For late joiner recovery, use a durable store and a projection service:
1. Durable store: write events to a database or outbox table as part of your domain transaction.
2. Projection: background worker reads events and transforms them into `SignalRTransportEnvelope` payloads.
3. SignalR: publish the projected payloads to the correct users/groups.

Example sketch:
```csharp
// Durable store write (e.g., in application layer)
await _eventStore.AppendAsync(new MyRealtimeEvent(order.Id, messageType, payload, targets), ct);

// Projection worker
IReadOnlyList<MyRealtimeEvent> events = await _eventStore.GetSinceAsync(cursor, ct);
foreach (MyRealtimeEvent evt in events)
{
    await _hub.Clients.Users(evt.Targets.Users)
        .SendAsync("Publish", evt.Envelope, ct);
}
```

## Scale-out / backplane guidance
- Azure SignalR Service: simplest managed scale-out with consistent fan-out.
- Redis backplane: good for self-hosted scale-out; ensure proper connection limits and health checks.
- SQL Server backplane: acceptable for low-volume deployments; higher latency than Redis.

Without a backplane, only clients connected to the same server instance will receive events.
