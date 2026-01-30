# Sol9.SseClient

Console client to verify SSE published events from Orders.API (Transponder.Transport.SSE).

## Usage

1. **Start Orders.API** (e.g. `dotnet run --project Orders.API`)

2. **Run the SSE client** in another terminal:
   ```bash
   dotnet run --project Sol9.SseClient
   ```

3. **Trigger events** by creating an order via the Orders API (e.g. POST to the orders endpoint). The client will display received SSE events.

## Options

| Option | Description | Default |
|--------|-------------|---------|
| `--url <baseUrl>` | Orders.API base URL | `http://localhost:5296` |
| `--stream <stream>` | Stream filter (query param) | `all` |

### Examples

```bash
# Default (localhost:5296, stream=all)
dotnet run --project Sol9.SseClient

# Custom URL
dotnet run --project Sol9.SseClient -- --url https://localhost:7003

# Specific stream
dotnet run --project Sol9.SseClient -- --stream orders
```
