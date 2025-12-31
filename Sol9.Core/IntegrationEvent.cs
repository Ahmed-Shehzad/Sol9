using Sol9.Core.Abstractions;

namespace Sol9.Core;

/// <summary>
/// Base type for integration events with standard metadata.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
