using Sol9.Eventing.Abstractions;

namespace Sol9.Eventing;

/// <summary>
/// Base type for domain events with standard metadata.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
