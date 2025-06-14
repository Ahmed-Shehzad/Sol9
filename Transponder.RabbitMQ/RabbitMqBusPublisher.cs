using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Transponder.Core.Abstractions;

namespace Transponder.RabbitMQ;

public class RabbitMqBusPublisher : IBusPublisher
{
    private readonly IRabbitMqConnectionFactory _rabbitMqConnectionFactory;

    public RabbitMqBusPublisher(IRabbitMqConnectionFactory rabbitMqConnectionFactory)
    {
        _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
    }
    
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent
    {
        var connection = await _rabbitMqConnectionFactory.GetConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        // Declare exchanges
        var mainExchange = $"main_exchange_{typeof(TEvent).Name}";
        var retryExchange = $"retry_exchange_{typeof(TEvent).Name}";
        var deadLetterExchange = $"dead_letter_exchange_{typeof(TEvent).Name}";
        
        var mainExchangeTask = channel.ExchangeDeclareAsync(mainExchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        var retryExchangeTask = channel.ExchangeDeclareAsync(retryExchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        var deadLetterExchangeTask = channel.ExchangeDeclareAsync(deadLetterExchange, ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken);

        await Task.WhenAll(mainExchangeTask, retryExchangeTask, deadLetterExchangeTask);
        
        // Declare main queue with DLX
        var mainQueue = $"main_queue_{typeof(TEvent).Name}";
        var retryQueue = $"retry_queue_{typeof(TEvent).Name}";
        var deadLetterQueue = $"dead_letter_queue_{typeof(TEvent).Name}";
        
        var mainQueueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", retryExchange }
        };
        await channel.QueueDeclareAsync(mainQueue, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs!, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(mainQueue, mainExchange, routingKey: "", cancellationToken: cancellationToken);
        
        // Declare retry queue with TTL and DLX back to main exchange
        var retryQueueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", mainExchange },
            { "x-message-ttl", 5000 } // 5 seconds delay
        };
        await channel.QueueDeclareAsync(retryQueue, durable: true, exclusive: false, autoDelete: false, arguments: retryQueueArgs!, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(retryQueue, retryExchange, routingKey: "", cancellationToken: cancellationToken);

        // Declare dead letter queue
        await channel.QueueDeclareAsync(deadLetterQueue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(deadLetterQueue, deadLetterExchange, routingKey: "", cancellationToken: cancellationToken);
        
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            Headers = new Dictionary<string, object>()!,
            Expiration = "36000000",
            DeliveryMode = DeliveryModes.Persistent,
            Persistent = true
        };
        
        await channel.BasicPublishAsync(exchange: mainExchange, routingKey: "", mandatory: true, basicProperties: properties, body: body, cancellationToken: cancellationToken);
    }
}