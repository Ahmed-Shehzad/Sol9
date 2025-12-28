using Sol9.Eventing.Abstractions;

namespace Sol9.Eventing;

/// <summary>
/// Base type for integration events with standard metadata.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
