using Transponder.Persistence.Abstractions;

namespace Transponder.Persistence;

/// <summary>
/// Default outbox message implementation.
/// </summary>
public sealed class OutboxMessage : IOutboxMessage
{
    private readonly byte[] _body;
    private readonly IReadOnlyDictionary<string, object?> _headers;

    public OutboxMessage(
        Guid messageId,
        ReadOnlyMemory<byte> body,
        IReadOnlyDictionary<string, object?>? headers = null,
        DateTimeOffset? enqueuedTime = null,
        Guid? correlationId = null,
        Guid? conversationId = null,
        Uri? sourceAddress = null,
        Uri? destinationAddress = null,
        string? messageType = null,
        string? contentType = null,
        DateTimeOffset? sentTime = null)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("MessageId must be provided.", nameof(messageId));
        }

        MessageId = messageId;
        CorrelationId = correlationId;
        ConversationId = conversationId;
        SourceAddress = sourceAddress;
        DestinationAddress = destinationAddress;
        MessageType = messageType;
        ContentType = contentType;
        _body = body.ToArray();
        _headers = headers != null
            ? new Dictionary<string, object?>(headers, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        EnqueuedTime = enqueuedTime ?? DateTimeOffset.UtcNow;
        SentTime = sentTime;
    }

    /// <inheritdoc />
    public Guid MessageId { get; }

    /// <inheritdoc />
    public Guid? CorrelationId { get; }

    /// <inheritdoc />
    public Guid? ConversationId { get; }

    /// <inheritdoc />
    public Uri? SourceAddress { get; }

    /// <inheritdoc />
    public Uri? DestinationAddress { get; }

    /// <inheritdoc />
    public string? MessageType { get; }

    /// <inheritdoc />
    public string? ContentType { get; }

    /// <inheritdoc />
    public ReadOnlyMemory<byte> Body => _body;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Headers => _headers;

    /// <inheritdoc />
    public DateTimeOffset EnqueuedTime { get; }

    /// <inheritdoc />
    public DateTimeOffset? SentTime { get; private set; }

    internal void MarkSent(DateTimeOffset sentTime)
    {
        SentTime = sentTime;
    }

    public static OutboxMessage FromMessage(IOutboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new OutboxMessage(
            message.MessageId,
            message.Body,
            message.Headers,
            message.EnqueuedTime,
            message.CorrelationId,
            message.ConversationId,
            message.SourceAddress,
            message.DestinationAddress,
            message.MessageType,
            message.ContentType,
            message.SentTime);
    }
}
