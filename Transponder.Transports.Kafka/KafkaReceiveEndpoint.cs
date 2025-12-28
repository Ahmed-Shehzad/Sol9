using Confluent.Kafka;

using Polly;

using Transponder.Transports.Abstractions;
using Transponder.Transports.Kafka.Abstractions;

namespace Transponder.Transports.Kafka;

internal sealed class KafkaReceiveEndpoint : IReceiveEndpoint
{
    private readonly Func<IReceiveContext, Task> _handler;
    private readonly IKafkaHostSettings _settings;
    private readonly Uri _inputAddress;
    private readonly Uri _hostAddress;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly KafkaSendTransport? _deadLetterTransport;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public KafkaReceiveEndpoint(
        IReceiveEndpointConfiguration configuration,
        IKafkaHostSettings settings,
        Uri hostAddress,
        IProducer<string, byte[]> producer,
        ReceiveEndpointFaultSettings? faultSettings,
        ResiliencePipeline resiliencePipeline)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(hostAddress);
        ArgumentNullException.ThrowIfNull(producer);
        ArgumentNullException.ThrowIfNull(resiliencePipeline);

        _handler = configuration.Handler ?? throw new ArgumentNullException(nameof(configuration.Handler));
        _inputAddress = configuration.InputAddress;
        _settings = settings;
        _hostAddress = hostAddress;
        _resiliencePipeline = resiliencePipeline;
        _deadLetterTransport = faultSettings?.DeadLetterAddress is null
            ? null
            : new KafkaSendTransport(producer, settings.Topology.GetTopicName(faultSettings.DeadLetterAddress));
    }

    public Uri InputAddress => _inputAddress;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_loop is not null) return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_cts is null) return;

        await _cts.CancelAsync();
        if (_loop is not null) await _loop.ConfigureAwait(false);

        _cts.Dispose();
        _cts = null;
        _loop = null;
    }

    public ValueTask DisposeAsync() => new(StopAsync());

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        string topic = _settings.Topology.GetTopicName(_inputAddress);
        string groupId = _settings.Topology.GetConsumerGroup(_inputAddress);
        ConsumerConfig config = KafkaTransportHost.BuildConsumerConfig(_settings, groupId);

        using IConsumer<string, byte[]>? consumer = new ConsumerBuilder<string, byte[]>(config).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, byte[]>? result = consumer.Consume(cancellationToken);
                if (result?.Message is null) continue;

                Dictionary<string, object?> headers = KafkaTransportHeaders.ReadHeaders(result.Message.Headers);
                string? contentType = headers.TryGetValue("ContentType", out object? ct) ? ct as string : null;
                headers.Remove("ContentType");
                string? messageType = headers.TryGetValue("MessageType", out object? mt) ? mt as string : null;
                headers.Remove("MessageType");
                Guid? correlationId = headers.TryGetValue("CorrelationId", out object? corr)
                                      && Guid.TryParse(corr?.ToString(), out Guid parsedCorrelationId)
                    ? parsedCorrelationId
                    : (Guid?)null;
                headers.Remove("CorrelationId");
                Guid? conversationId = headers.TryGetValue("ConversationId", out object? conv)
                                       && Guid.TryParse(conv?.ToString(), out Guid parsedConversationId)
                    ? parsedConversationId
                    : (Guid?)null;
                headers.Remove("ConversationId");

                var transportMessage = new TransportMessage(
                    result.Message.Value ?? [],
                    contentType,
                    headers,
                    Guid.TryParse(result.Message.Key, out Guid messageId) ? messageId : null,
                    correlationId,
                    conversationId,
                    messageType,
                    null);

                var context = new KafkaReceiveContext(
                    transportMessage,
                    _hostAddress,
                    _inputAddress,
                    cancellationToken);

                try
                {
                    await _resiliencePipeline.ExecuteAsync(
                            async _ => await _handler(context).ConfigureAwait(false),
                            cancellationToken)
                        .ConfigureAwait(false);
                    consumer.Commit(result);
                }
                catch
                {
                    if (_deadLetterTransport is null) continue;

                    try
                    {
                        await _deadLetterTransport.SendAsync(transportMessage, cancellationToken)
                            .ConfigureAwait(false);
                        consumer.Commit(result);
                    }
                    catch
                    {
                        // Leave offset uncommitted for retry if DLQ dispatch fails.
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            consumer.Close();
        }
    }
}
