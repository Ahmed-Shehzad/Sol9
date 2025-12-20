using RabbitMQ.Client;
using Transponder.Transports.Abstractions;
using Transponder.Transports.RabbitMq.Abstractions;

namespace Transponder.Transports.RabbitMq;

internal sealed class RabbitMqPublishTransport : IPublishTransport
{
    private readonly IConnection _connection;
    private readonly string _exchangeName;
    private readonly IRabbitMqTopology _topology;
    private readonly Type _messageType;

    public RabbitMqPublishTransport(
        IConnection connection,
        string exchangeName,
        IRabbitMqTopology topology,
        Type messageType)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _exchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        _topology = topology ?? throw new ArgumentNullException(nameof(topology));
        _messageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
    }

    public Task PublishAsync(ITransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        using var channel = _connection.CreateModel();
        channel.ExchangeDeclare(_exchangeName, _topology.ExchangeType, durable: true, autoDelete: false);

        var properties = channel.CreateBasicProperties();
        properties.ContentType = message.ContentType;
        properties.MessageId = message.MessageId?.ToString();
        properties.CorrelationId = message.CorrelationId?.ToString();
        properties.Headers = RabbitMqTransportHeaders.BuildHeaders(message);

        channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: _topology.GetRoutingKey(_messageType),
            mandatory: false,
            basicProperties: properties,
            body: message.Body.ToArray());

        return Task.CompletedTask;
    }
}
