using Polly;
using RabbitMQ.Client;
using Transponder.Transports.Abstractions;
using Transponder.Transports.RabbitMq.Abstractions;

namespace Transponder.Transports.RabbitMq;

/// <summary>
/// RabbitMQ transport host backed by AMQP connections.
/// </summary>
public sealed class RabbitMqTransportHost : TransportHostBase
{
    private readonly IConnection _connection;
    private readonly List<RabbitMqReceiveEndpoint> _receiveEndpoints = [];
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly TransportResilienceOptions? _resilienceOptions;

    public RabbitMqTransportHost(IRabbitMqHostSettings settings)
        : base(settings?.Address ?? throw new ArgumentNullException(nameof(settings)))
    {
        Settings = settings;
        _resilienceOptions = (settings as ITransportHostResilienceSettings)?.ResilienceOptions;
        _resiliencePipeline = TransportResiliencePipeline.Create(_resilienceOptions);
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            VirtualHost = settings.VirtualHost,
            DispatchConsumersAsync = true
        };

        if (!string.IsNullOrWhiteSpace(settings.Username))
        {
            factory.UserName = settings.Username;
        }

        if (!string.IsNullOrWhiteSpace(settings.Password))
        {
            factory.Password = settings.Password;
        }

        if (settings.UseTls)
        {
            factory.Ssl.Enabled = true;
        }

        if (settings.RequestedHeartbeat.HasValue)
        {
            factory.RequestedHeartbeat = settings.RequestedHeartbeat.Value;
        }

        _connection = factory.CreateConnection();
    }

    public IRabbitMqHostSettings Settings { get; }

    public override Task<ISendTransport> GetSendTransportAsync(
        Uri address,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(address);
        var queueName = Settings.Topology.GetQueueName(address);
        var transport = new RabbitMqSendTransport(_connection, queueName);
        ISendTransport resilientTransport = TransportResiliencePipeline.WrapSend(transport, _resiliencePipeline);
        return Task.FromResult(resilientTransport);
    }

    public override Task<IPublishTransport> GetPublishTransportAsync(
        Type messageType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        var exchangeName = Settings.Topology.GetExchangeName(messageType);
        var transport = new RabbitMqPublishTransport(
            _connection,
            exchangeName,
            Settings.Topology,
            messageType);
        IPublishTransport resilientTransport = TransportResiliencePipeline.WrapPublish(transport, _resiliencePipeline);
        return Task.FromResult(resilientTransport);
    }

    public override IReceiveEndpoint ConnectReceiveEndpoint(IReceiveEndpointConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var faultSettings = ReceiveEndpointFaultSettingsResolver.Resolve(configuration);
        var pipeline = TransportResiliencePipeline.Create(faultSettings?.ResilienceOptions ?? _resilienceOptions);
        var endpoint = new RabbitMqReceiveEndpoint(
            _connection,
            configuration,
            Settings.Topology,
            Address,
            faultSettings,
            pipeline);
        _receiveEndpoints.Add(endpoint);
        return endpoint;
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var endpoint in _receiveEndpoints)
        {
            await endpoint.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public override async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _connection.Dispose();
    }
}
