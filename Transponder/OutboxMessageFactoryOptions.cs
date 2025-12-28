namespace Transponder;

public sealed class OutboxMessageFactoryOptions
{
    public Guid? MessageId { get; init; }

    public Guid? CorrelationId { get; init; }

    public Guid? ConversationId { get; init; }

    public Uri? SourceAddress { get; init; }

    public Uri? DestinationAddress { get; init; }

    public IReadOnlyDictionary<string, object?>? Headers { get; init; }
}
