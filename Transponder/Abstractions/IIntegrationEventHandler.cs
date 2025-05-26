namespace Transponder.Abstractions;

/// <summary>
/// Represents a handler for processing integration events.
/// </summary>
/// <typeparam name="TEvent">The type of integration event to handle. Must implement IIntergationEvent.</typeparam>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    /// <summary>
    /// Handles the notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification instance to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TEvent notification, CancellationToken cancellationToken);
}