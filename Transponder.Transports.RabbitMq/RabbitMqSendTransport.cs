using RabbitMQ.Client;
using Transponder.Transports.Abstractions;

namespace Transponder.Transports.RabbitMq;

internal sealed class RabbitMqSendTransport : ISendTransport
{
    private readonly IConnection _connection;
    private readonly string _queueName;

    public RabbitMqSendTransport(IConnection connection, string queueName)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
    }

    public Task SendAsync(ITransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        using IModel? channel = _connection.CreateModel();
        channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

        IBasicProperties? properties = channel.CreateBasicProperties();
        properties.ContentType = message.ContentType;
        properties.MessageId = message.MessageId?.ToString();
        properties.CorrelationId = message.CorrelationId?.ToString();
        properties.Headers = RabbitMqTransportHeaders.BuildHeaders(message);

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: _queueName,
            mandatory: false,
            basicProperties: properties,
            body: message.Body.ToArray());

        return Task.CompletedTask;
    }
}
