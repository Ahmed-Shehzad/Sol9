using Intercessor.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sol9.Core.Types;

namespace Transponder.Storage.EntityFrameworkCore.Interceptors;

public class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public PublishDomainEventsInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        _ = PublishDomainEventsAsync(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        _ = PublishDomainEventsAsync(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Publishes all domain events from aggregate roots tracked by the given <see cref="DbContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> whose tracked aggregate roots' domain events will be published.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task PublishDomainEventsAsync(DbContext? context)
    {
        if (context == null) return;
        
        var aggregateRoots = context.ChangeTracker.Entries<AggregateRoot<Ulid>>().ToList();
        var domainEvents = aggregateRoots
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();
        
        aggregateRoots.ForEach(d => 
        {
            d.Entity.ClearDomainEvents();
        });
        
        var domainEventTasks = domainEvents.Select(domainEvent => _publisher.PublishAsync(domainEvent)).ToList();
        
        await Task.WhenAll(domainEventTasks);
    }
}