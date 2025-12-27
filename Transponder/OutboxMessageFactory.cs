using Transponder.Abstractions;
using Transponder.Persistence;
using Transponder.Transports.Abstractions;

namespace Transponder;

internal static class OutboxMessageFactory
{
    public static OutboxMessage Create<TMessage>(
        TMessage message,
        IMessageSerializer serializer,
        Guid? messageId,
        Guid? correlationId,
        Guid? conversationId,
        Uri? sourceAddress,
        Uri? destinationAddress,
        IReadOnlyDictionary<string, object?>? headers)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(serializer);

        Type messageType = message.GetType();
        ReadOnlyMemory<byte> body = serializer.Serialize(message, messageType);
        string typeName = messageType.FullName ?? messageType.Name;
        var headerMap = headers is null
            ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(headers, StringComparer.OrdinalIgnoreCase);

        return new OutboxMessage(
            messageId ?? Guid.NewGuid(),
            body,
            headerMap,
            enqueuedTime: DateTimeOffset.UtcNow,
            correlationId: correlationId,
            conversationId: conversationId,
            sourceAddress: sourceAddress,
            destinationAddress: destinationAddress,
            messageType: typeName,
            contentType: serializer.ContentType,
            sentTime: null);
    }
}
