// Quick SSE client to verify Transponder.Transport.SSE published events from Orders.API.
// Usage: dotnet run [--url <baseUrl>] [--stream <stream>]
// Example: dotnet run -- --url http://localhost:5296 --stream all

using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

string baseUrl = "http://localhost:5296";
string stream = "all";

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--url" && i + 1 < args.Length)
    {
        baseUrl = args[++i].TrimEnd('/');
        continue;
    }

    if (args[i] == "--stream" && i + 1 < args.Length)
    {
        stream = args[++i];
        continue;
    }
}

string sseUrl = $"{baseUrl}/transponder/stream?stream={Uri.EscapeDataString(stream)}";
Console.WriteLine($"Connecting to SSE: {sseUrl}");
Console.WriteLine("Waiting for events... (create an order via Orders.API to trigger events)");
Console.WriteLine(new string('-', 60));

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    await ConsumeSseAsync(sseUrl, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nDisconnected.");
}
catch (Exception ex)
{
    Console.WriteLine($"\nError: {ex.Message}");
    if (Debugger.IsAttached)
        throw;
    Environment.Exit(1);
}

async static Task ConsumeSseAsync(string url, CancellationToken cancellationToken)
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
    http.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

    await using var responseStream = await http.GetStreamAsync(url, cancellationToken);
    using var reader = new StreamReader(responseStream, Encoding.UTF8);

    string? eventName = null;
    string? eventId = null;
    var dataLines = new List<string>();

    while (!cancellationToken.IsCancellationRequested)
    {
        string? line = await reader.ReadLineAsync(cancellationToken);
        if (line is null)
            break;

        if (string.IsNullOrEmpty(line))
        {
            if (dataLines.Count > 0 || eventName is not null || eventId is not null)
            {
                string data = string.Join("\n", dataLines);
                PrintEvent(eventName ?? "message", eventId, data);
            }

            eventName = null;
            eventId = null;
            dataLines.Clear();
            continue;
        }

        if (line.StartsWith("event:", StringComparison.OrdinalIgnoreCase))
        {
            eventName = line.Length > 6 ? line[6..].Trim() : null;
            continue;
        }

        if (line.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
        {
            eventId = line.Length > 3 ? line[3..].Trim() : null;
            continue;
        }

        if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            string value = line.Length > 5 ? line[5..] : string.Empty;
            if (value.Length > 0 && value[0] == ' ')
                value = value[1..];
            dataLines.Add(value);
            continue;
        }

        if (line.StartsWith(":", StringComparison.Ordinal))
        {
            string comment = line.Length > 1 ? line[1..].Trim() : string.Empty;
            if (!string.IsNullOrEmpty(comment))
                Console.WriteLine($"[comment] {comment}");
        }
    }
}

static void PrintEvent(string eventName, string? eventId, string data)
{
    if (string.IsNullOrWhiteSpace(data))
        return;

    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine();
    Console.WriteLine($"[{timestamp}] event: {eventName}" + (string.IsNullOrEmpty(eventId) ? "" : $" id: {eventId}"));

    try
    {
        using var doc = JsonDocument.Parse(data);
        var root = doc.RootElement;

        if (root.TryGetProperty("messageType", out var mt))
            Console.WriteLine($"  messageType: {mt.GetString()}");

        if (root.TryGetProperty("sentTime", out var st) && st.ValueKind == JsonValueKind.String)
            Console.WriteLine($"  sentTime: {st.GetString()}");

        if (root.TryGetProperty("body", out var bodyProp))
        {
            string? bodyB64 = bodyProp.GetString();
            if (!string.IsNullOrEmpty(bodyB64))
                try
                {
                    byte[] bytes = Convert.FromBase64String(bodyB64);
                    string bodyStr = Encoding.UTF8.GetString(bytes);

                    if (bodyStr.StartsWith('{') || bodyStr.StartsWith('['))
                    {
                        using var bodyDoc = JsonDocument.Parse(bodyStr);
                        Console.WriteLine($"  body: {JsonSerializer.Serialize(bodyDoc.RootElement, new JsonSerializerOptions { WriteIndented = false })}");
                    }
                    else Console.WriteLine($"  body: {bodyStr}");
                }
                catch
                {
                    Console.WriteLine($"  body: (base64, {bodyB64.Length} chars)");
                }
        }

        if (root.TryGetProperty("headers", out var headers) && headers.ValueKind == JsonValueKind.Object)
        {
            int count = 0;
            foreach (var p in headers.EnumerateObject())
            {
                if (count == 0)
                    Console.WriteLine("  headers:");
                Console.WriteLine($"    {p.Name}: {p.Value}");
                count++;
            }
        }
    }
    catch (JsonException)
    {
        Console.WriteLine($"  data: {data}");
    }

    Console.WriteLine(new string('-', 60));
}
