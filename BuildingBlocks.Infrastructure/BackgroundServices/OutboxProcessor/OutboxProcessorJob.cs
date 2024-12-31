using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.BackgroundServices.OutboxProcessor;

/// <summary>
/// A Hangfire job that processes outbox messages from a database.
/// The job retrieves unprocessed events, deserializes them, and publishes them using MediatR.
/// After processing, the event is marked as processed in the outbox.
/// </summary>
public class OutboxProcessorJob(IServiceProvider serviceProvider)
{
    public async Task ExecuteAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var unprocessedEvents = await outboxService.GetUnprocessedEventsAsync();

        foreach (var outboxMessage in unprocessedEvents)
        {
            var domainEventType = scope.ServiceProvider.GetServices<IDomainEvent>()
                .FirstOrDefault(t => t.GetType().Name.Equals(outboxMessage.Type))?.GetType();

            if (domainEventType == null) continue;

            var jsonObject = JsonSerializer.Deserialize(outboxMessage.Payload, domainEventType);
            if (jsonObject == null) continue;

            var @event = (IDomainEvent)jsonObject;
            await mediator.Publish(@event, default);
            await outboxService.MarkEventAsProcessedAsync(outboxMessage.Id);
        }
    }
}