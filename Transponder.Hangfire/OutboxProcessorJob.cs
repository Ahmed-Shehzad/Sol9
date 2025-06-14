using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transponder.Core.Abstractions;
using Transponder.Storage.Outbox;

namespace Transponder.Hangfire;

public class OutboxProcessorJob : IOutboxProcessorJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorJob> _logger;

    public OutboxProcessorJob(IServiceProvider serviceProvider, ILogger<OutboxProcessorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ProcessOutboxAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
        var publisher = scope.ServiceProvider.GetRequiredService<IBusPublisher>();

        var messages = await outboxService.GetUnprocessedMessagesAsync(cancellationToken);
        var processedMessages = new List<OutboxMessage>();
        
        foreach (var message in messages)
        {
            var @event = message.GetMessage<IIntegrationEvent>();
            if (@event == null) continue;
            
            try
            {
                _logger.LogInformation("OutboxProcessorJob: Processing message {MessageId}", message.Id);
                await publisher.PublishAsync(@event, cancellationToken);
                message.MarkAsPublished();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxProcessorJob: Error processing message {MessageId}", message.Id);
                message.MarkAsFailed(ex);
            }
            finally
            {
                processedMessages.Add(message);
            }
        }
        await outboxService.MarkAsProcessedAsync(processedMessages, cancellationToken);
    }
}