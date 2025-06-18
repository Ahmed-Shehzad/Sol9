using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transponder.Core.Abstractions;
using Transponder.Storage.Outbox;

namespace Transponder.Hangfire;

/// <summary>
/// Represents a Hangfire job responsible for processing outbox messages and publishing integration events.
/// </summary>
public class OutboxProcessorJob : IOutboxProcessorJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxProcessorJob"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="logger">The logger instance for logging job activity.</param>
    public OutboxProcessorJob(IServiceProvider serviceProvider, ILogger<OutboxProcessorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Processes unprocessed outbox messages, publishes integration events, and marks them as processed.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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