using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Infrastructure.Repositories;

namespace BuildingBlocks.Infrastructure.Services;

public class OutboxService(IOutboxRepository outboxRepository, IUnitOfWork unitOfWork) : IOutboxService
{
    /// <summary>
    /// Adds domain events to the outbox repository.
    /// </summary>
    /// <param name="domainEvents">The collection of domain events to be added.</param>
    public void AddDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = Outbox.Create(domainEvent.GetType().Name, JsonSerializer.Serialize(domainEvent));
            outboxRepository.Add(outboxMessage);
        }
    }

    /// <summary>
    /// Retrieves all unprocessed outbox messages asynchronously.
    /// </summary>
    /// <returns>An asynchronous task that returns an enumeration of unprocessed outbox messages.</returns>
    public async Task<IEnumerable<Outbox>> GetUnprocessedEventsAsync()
    {
        return await outboxRepository.FindAllByAsync(m => !m.Processed);
    }

    /// <summary>
    /// Marks a specific outbox message as processed and commits the changes asynchronously.
    /// </summary>
    /// <param name="messageId">The unique identifier of the outbox message to be marked as processed.</param>
    /// <returns>An asynchronous task.</returns>
    public async Task MarkEventAsProcessedAsync(Ulid messageId)
    {
        var outboxMessage = await outboxRepository.FindAsync(m => m.Id.Equals(messageId));
        if (outboxMessage != null)
        {
            outboxMessage.MarkAsProcessed();
            outboxRepository.Update(outboxMessage);
            await unitOfWork.CommitAsync();
        }
    }
}