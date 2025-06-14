using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sol9.Core.Types;
using Transponder.Storage.Outbox;

namespace Transponder.Storage.EntityFrameworkCore.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // Here you can implement logic to handle outbox pattern
        SaveOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        // Here you can implement logic to handle outbox pattern
        SaveOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    /// <summary>
    /// Extracts integration events from all tracked <see cref="AggregateRoot{TKey}"/> entities,
    /// creates corresponding <see cref="OutboxMessage"/> instances, clears the events from the aggregates,
    /// and adds the messages to the <see cref="DbContext"/> for persistence.
    /// </summary>
    /// <param name="context">
    /// The <see cref="DbContext"/> instance whose tracked entities will be inspected for integration events.
    /// </param>
    private static void SaveOutboxMessages(DbContext? context)
    {
        if (context == null) return;

        var aggregateRoots = context.ChangeTracker.Entries<AggregateRoot<Ulid>>().ToList();

        var outboxMessages = aggregateRoots
            .SelectMany(e => 
                e.Entity.IntegrationEvents.Select(OutboxMessage.Create))
            .ToList();

        aggregateRoots.ForEach(e =>
        {
            e.Entity.ClearIntegrationEvents();
        });

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}