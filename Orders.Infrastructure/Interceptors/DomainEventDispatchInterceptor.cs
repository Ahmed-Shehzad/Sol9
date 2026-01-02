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

    public async override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is not null) await DispatchDomainEventsAsync(eventData.Context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null) DispatchDomainEventsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
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
