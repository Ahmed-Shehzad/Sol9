using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Infrastructure.BackgroundServices.OutboxProcessor;

/// <summary>
/// A background service that processes outbox messages from a database.
/// The service retrieves unprocessed events, deserializes them, and publishes them using MediatR.
/// After processing, the event is marked as processed in the outbox.
/// </summary>
public class OutboxProcessorBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
            {
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
            // Wait some time before checking again for new events
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}