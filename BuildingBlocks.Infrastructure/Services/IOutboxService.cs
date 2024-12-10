using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;

namespace BuildingBlocks.Infrastructure.Services;

/// <summary>
/// Defines a service for managing domain events that need to be persisted and processed.
/// </summary>
public interface IOutboxService
{
    /// <summary>
    /// Adds a collection of domain events to the outbox.
    /// </summary>
    /// <param name="domainEvents">The collection of domain events to be added.</param>
    void AddDomainEvents(IEnumerable<IDomainEvent> domainEvents);

    /// <summary>
    /// Retrieves a collection of unprocessed events from the outbox.
    /// </summary>
    /// <returns>An asynchronous task that resolves to a collection of unprocessed events.</returns>
    Task<IEnumerable<Outbox>> GetUnprocessedEventsAsync();

    /// <summary>
    /// Marks a specific event as processed in the outbox.
    /// </summary>
    /// <param name="messageId">The unique identifier of the event to be marked as processed.</param>
    /// <returns>An asynchronous task that completes when the event is marked as processed.</returns>
    Task MarkEventAsProcessedAsync(Ulid messageId);
}