using Intercessor.Abstractions;

namespace Sol9.Core.Abstractions;

/// <summary>
/// Marker interface for integration events published outside the bounded context.
/// </summary>
public interface IIntegrationEvent : INotification;
