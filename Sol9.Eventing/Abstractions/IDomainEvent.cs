using Intercessor.Abstractions;

namespace Sol9.Eventing.Abstractions;

/// <summary>
/// Marker interface for domain events dispatched within the same bounded context.
/// </summary>
public interface IDomainEvent : INotification;
