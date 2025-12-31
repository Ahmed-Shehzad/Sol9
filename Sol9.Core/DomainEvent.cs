using Sol9.Core.Abstractions;

namespace Sol9.Core;

/// <summary>
/// Base type for domain events with standard metadata.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
