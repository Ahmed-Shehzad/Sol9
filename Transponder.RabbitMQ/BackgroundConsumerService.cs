using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Transponder.Abstractions;

namespace Transponder.RabbitMQ;

public class BackgroundConsumerService<TNotification> : BackgroundService, IBusConsumer<TNotification> where TNotification : IIntegrationEvent
{
    private readonly IServiceProvider  _serviceProvider;
    private readonly Dictionary<Type, object> _channels;
    
    public BackgroundConsumerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _channels = new Dictionary<Type, object>();
    }
    
    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            await foreach (var item in ConsumeAsync<TNotification>(cancellationToken))
            {
                var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TNotification>>();
                await handler.HandleAsync(item, cancellationToken);
            }
        }
    }

    private Channel<TNotification> GetOrCreateChannel<TNotification>() where TNotification : IIntegrationEvent
    {
        var type = typeof(TNotification);
        if (_channels.TryGetValue(type, out var obj))
        {
            return (Channel<TNotification>)obj;
        }

        var newChannel = Channel.CreateUnbounded<TNotification>();
        _channels[type] = newChannel;
        return newChannel;
    }
    
    public async IAsyncEnumerable<TNotification> ConsumeAsync<TNotification>([EnumeratorCancellation] CancellationToken cancellationToken = default) where TNotification : IIntegrationEvent
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var rabbitMqConnectionFactory = scope.ServiceProvider.GetRequiredService<IRabbitMqConnectionFactory>();
        var connection = await rabbitMqConnectionFactory.GetConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        // Declare main queue with DLX
        var mainQueue = $"main_queue_{typeof(TNotification).Name}";

        var consumer = new AsyncEventingBasicConsumer(channel);

        var messageQueue = GetOrCreateChannel<TNotification>();

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<TNotification>(Encoding.UTF8.GetString(body));

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    // Process the message
                    if (message != null)
                    {
                        await messageQueue.Writer.WriteAsync(message, cancellationToken);
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                    }
                });
            }
            catch
            {
                // After retries, reject the message and send to DLQ
                await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, cancellationToken: cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(queue: mainQueue, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        await foreach (var item in messageQueue.Reader.ReadAllAsync(cancellationToken))
        {
            yield return item;
        }
    }
}