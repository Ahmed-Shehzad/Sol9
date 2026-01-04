using Sol9.Core.Abstractions;

namespace Sol9.Core;

/// <summary>
/// Base type for integration events with standard metadata.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
        EventId = Guid.NewGuid();
        OccurredOn= DateTime.UtcNow;
    }

    public Guid EventId { get; init; }
    public DateTimeOffset OccurredOn { get; init; }
    public Guid CorrelationId { get; }
}
