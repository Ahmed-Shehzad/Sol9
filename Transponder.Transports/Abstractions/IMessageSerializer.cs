namespace Transponder.Transports.Abstractions;

/// <summary>
/// Defines serialization for transport message bodies.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Gets the content type produced by this serializer.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Serializes a message to a byte payload.
    /// </summary>
    /// <param name="message">The message instance to serialize.</param>
    /// <param name="messageType">The message type.</param>
    ReadOnlyMemory<byte> Serialize(object message, Type messageType);

    /// <summary>
    /// Deserializes a byte payload into a message instance.
    /// </summary>
    /// <param name="body">The message payload bytes.</param>
    /// <param name="messageType">The message type.</param>
    object Deserialize(ReadOnlySpan<byte> body, Type messageType);
}
