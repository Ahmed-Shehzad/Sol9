using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Sol9.Core;

namespace Orders.Infrastructure.Interceptors;

public sealed class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DomainEventDispatchInterceptor(IPublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    // ASYNC PATH
    public async override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null) await DispatchDomainEventsAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    // SYNC PATH
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not null)
            DispatchDomainEventsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();

        return base.SavedChanges(eventData, result);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Only aggregates that actually changed
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e =>
                e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        // Preserve ordering
        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        await Task.WhenAll(domainEvents.Select(domainEvent => _publisher.PublishAsync(domainEvent, cancellationToken)));

        // Clear only AFTER successful dispatch
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());
    }
}
