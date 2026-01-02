using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Sol9.Core;

namespace Bookings.Infrastructure.Interceptors;

public class IntegrationEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public IntegrationEventDispatcherInterceptor(IPublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is not null) await DispatchIntegrationEventsAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not null) DispatchIntegrationEventsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();

        return base.SavedChanges(eventData, result);
    }

    private async Task DispatchIntegrationEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Only aggregates that actually changed
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e =>
                e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .Where(e => e.IntegrationEvents.Count != 0)
            .ToList();

        // Preserve ordering
        var integrationEvents = aggregates
            .SelectMany(a => a.IntegrationEvents)
            .ToList();

        await Task.WhenAll(integrationEvents.Select(integrationEvent => _publisher.PublishAsync(integrationEvent, cancellationToken)));

        // Clear only AFTER successful dispatch
        aggregates.ForEach(aggregate => aggregate.ClearIntegrationEvents());
    }
}
