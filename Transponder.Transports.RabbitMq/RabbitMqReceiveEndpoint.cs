using Polly;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Transponder.Transports.Abstractions;
using Transponder.Transports.RabbitMq.Abstractions;

namespace Transponder.Transports.RabbitMq;

internal sealed class RabbitMqReceiveEndpoint : IReceiveEndpoint
{
    private readonly IConnection _connection;
    private readonly Func<IReceiveContext, Task> _handler;
    private readonly string _queueName;
    private readonly Uri _inputAddress;
    private readonly Uri _hostAddress;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly string? _deadLetterQueueName;
    private IModel? _channel;
    private string? _consumerTag;

    public RabbitMqReceiveEndpoint(
        IConnection connection,
        IReceiveEndpointConfiguration configuration,
        IRabbitMqTopology topology,
        Uri hostAddress,
        ReceiveEndpointFaultSettings? faultSettings,
        ResiliencePipeline resiliencePipeline)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(hostAddress);
        ArgumentNullException.ThrowIfNull(resiliencePipeline);

        _inputAddress = configuration.InputAddress;
        _handler = configuration.Handler ?? throw new ArgumentNullException(nameof(configuration.Handler));
        _queueName = topology.GetQueueName(_inputAddress);
        _hostAddress = hostAddress;
        _resiliencePipeline = resiliencePipeline;
        _deadLetterQueueName = faultSettings?.DeadLetterAddress is null
            ? null
            : topology.GetQueueName(faultSettings.DeadLetterAddress);
    }

    public Uri InputAddress => _inputAddress;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is not null) return Task.CompletedTask;

        _channel = _connection.CreateModel();
        IDictionary<string, object>? arguments = null;

        if (!string.IsNullOrWhiteSpace(_deadLetterQueueName))
        {
            _ = _channel.QueueDeclare(_deadLetterQueueName, durable: true, exclusive: false, autoDelete: false);
            arguments = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = _deadLetterQueueName
            };
        }

        _ = _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnReceivedAsync;

        _consumerTag = _channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is null) return Task.CompletedTask;

        if (!string.IsNullOrWhiteSpace(_consumerTag)) _channel.BasicCancel(_consumerTag);

        _channel.Close();
        _channel.Dispose();
        _channel = null;
        _consumerTag = null;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => new(StopAsync());

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null) return;

        Dictionary<string, object?> headers = RabbitMqTransportHeaders.ReadHeaders(args.BasicProperties.Headers);
        string? contentType = args.BasicProperties.ContentType;
        string? messageType = headers.TryGetValue("MessageType", out object? typeValue) ? typeValue as string : null;
        _ = headers.Remove("MessageType");
        Guid? conversationId = headers.TryGetValue("ConversationId", out object? conv)
                               && Guid.TryParse(conv?.ToString(), out Guid parsedConversationId)
            ? parsedConversationId
            : (Guid?)null;
        _ = headers.Remove("ConversationId");

        var transportMessage = new TransportMessage(
            args.Body.ToArray(),
            contentType,
            headers,
            Guid.TryParse(args.BasicProperties.MessageId, out Guid messageId) ? messageId : null,
            Guid.TryParse(args.BasicProperties.CorrelationId, out Guid correlationId) ? correlationId : null,
            conversationId,
            messageType,
            null);

        var context = new RabbitMqReceiveContext(
            transportMessage,
            _hostAddress,
            _inputAddress,
            CancellationToken.None);

        try
        {
            await _resiliencePipeline.ExecuteAsync(
                    async ct => await _handler(context).ConfigureAwait(false),
                    CancellationToken.None)
                .ConfigureAwait(false);
            _channel.BasicAck(args.DeliveryTag, false);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(_deadLetterQueueName)) _channel.BasicReject(args.DeliveryTag, false);
            else _channel.BasicNack(args.DeliveryTag, false, true);
        }
    }
}
