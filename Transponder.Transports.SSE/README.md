# Transponder.Transports.SSE

Server-Sent Events (SSE) transport for one-way realtime updates. Ideal for dashboards and live UI feeds.

## Semantics
- Server → client only (publish/send). No receive endpoints.
- Best-effort delivery; non-durable unless paired with a catch-up store.
- Works well behind proxies and HTTP/2.

## Registration
```csharp
services.AddTransponder(localAddress, options =>
{
    options.TransportBuilder.UseSse(localAddress);
});

app.MapTransponderSse();
```

## Targeting
Targets can be selected via headers:
- `Sse.Broadcast` (bool)
- `Sse.ConnectionId` / `Sse.ConnectionIds`
- `Sse.ExcludeConnectionId` / `Sse.ExcludeConnectionIds`
- `Sse.Stream` / `Sse.Streams`
- `Sse.Group` / `Sse.Groups`
- `Sse.User` / `Sse.Users`
- `Sse.EventName` (optional SSE event name override)
- `Sse.EventId` (optional SSE event id override)

If no target headers are provided, publish defaults to broadcast.

Send can also derive targets from destination addresses:
- `sse://connections/{id}`
- `sse://users/{id}`
- `sse://groups/{name}`
- `sse://streams/{name}`
- `sse://broadcast`

## SSE endpoint
`MapTransponderSse()` exposes an HTTP endpoint (default path: `/transponder/stream`) that accepts `text/event-stream`.

Query parameters:
- `stream` (comma/semicolon separated)
- `group` (comma/semicolon separated)
- `user`

If `stream` is omitted, the connection is registered under `all`.

## Payload format
Each SSE `data:` line is a JSON-serialized `SseTransportEnvelope` with `Body` encoded as base64.

## Connection event
By default, the endpoint emits an initial `connection` event with a JSON payload containing `connectionId`.

## Durable-ish catch-up (Last-Event-ID)
Browsers send `Last-Event-ID` on reconnect. You can supply a catch-up provider to replay missed events:
```csharp
services.AddSingleton<ISseCatchUpProvider, MyCatchUpProvider>();
```

The provider receives `SseCatchUpRequest` with:
- `LastEventId`
- `UserId`
- `Streams`
- `Groups`

You can also pass `lastEventId` as a query parameter for non-browser clients.

Replay events by returning `SseCatchUpEvent` instances.
`SseCatchUpEvent.Data` should be the exact payload you want sent in the SSE `data:` lines.

## Practical guidance
- Use SSE for UI/event feed style messages.
- Keep durable workflow messages on Kafka/Rabbit/Azure SB/etc.
- For strong UX, combine SSE with a catch-up store or event log.
