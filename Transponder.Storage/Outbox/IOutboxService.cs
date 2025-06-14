namespace Transponder.Storage.Outbox;

public interface IOutboxService
{
    /// <summary>
    /// Retrieves all unprocessed outbox messages.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of unprocessed <see cref="OutboxMessage"/> instances.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all outbox messages that have failed processing.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of failed <see cref="OutboxMessage"/> instances.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetFailedMessagesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a single outbox message to storage.
    /// </summary>
    /// <param name="message">The outbox message to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple outbox messages to storage.
    /// </summary>
    /// <param name="messages">The outbox messages to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified outbox messages as processed.
    /// </summary>
    /// <param name="messages">The outbox messages to mark as processed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task MarkAsProcessedAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default);
}