namespace Transponder.Abstractions;

/// <summary>
/// Marker interface for consumers.
/// </summary>
public interface IBusConsumer;

/// <summary>
/// Represents a consumer interface for handling notifications asynchronously.
/// </summary>
public interface IBusConsumer<TNotification> : IBusConsumer where TNotification : IIntegrationEvent
{
    /// <summary>
    /// Consumes the specified notification asynchronously.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to consume, must implement IIntergationEvent.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    IAsyncEnumerable<TNotification> ConsumeAsync<TNotification>(CancellationToken cancellationToken = default) where TNotification : IIntegrationEvent;
}