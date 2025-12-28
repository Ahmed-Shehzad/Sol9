using System.Text.Json;

using Transponder.Transports.Abstractions;

namespace Transponder;

/// <summary>
/// Serializes messages as JSON for transport payloads.
/// </summary>
public sealed class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    public JsonMessageSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    /// <inheritdoc />
    public string ContentType => "application/json";

    /// <inheritdoc />
    public ReadOnlyMemory<byte> Serialize(object message, Type messageType)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(messageType);

        return JsonSerializer.SerializeToUtf8Bytes(message, messageType, _options);
    }

    /// <inheritdoc />
    public object Deserialize(ReadOnlySpan<byte> body, Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        return JsonSerializer.Deserialize(body, messageType, _options)
            ?? throw new InvalidOperationException("Failed to deserialize message body.");
    }
}
