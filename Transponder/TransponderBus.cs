using Transponder.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder;

/// <summary>
/// Default bus implementation backed by transport hosts.
/// </summary>
public sealed class TransponderBus : IBusControl
{
    private readonly ITransportHostProvider _hostProvider;
    private readonly IReadOnlyCollection<ITransportHost> _hosts;
    private readonly IMessageSerializer _serializer;
    private readonly IMessageScheduler _scheduler;
    private readonly Func<Type, Uri?>? _requestAddressResolver;
    private readonly TimeSpan _defaultRequestTimeout;

    public TransponderBus(
        Uri address,
        ITransportHostProvider hostProvider,
        IEnumerable<ITransportHost> hosts,
        IMessageSerializer serializer,
        Func<Type, Uri?>? requestAddressResolver = null,
        TimeSpan? defaultRequestTimeout = null,
        Func<TransponderBus, IMessageScheduler>? schedulerFactory = null)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        _hostProvider = hostProvider ?? throw new ArgumentNullException(nameof(hostProvider));
        _hosts = hosts?.ToArray() ?? throw new ArgumentNullException(nameof(hosts));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _requestAddressResolver = requestAddressResolver;
        _defaultRequestTimeout = defaultRequestTimeout ?? TimeSpan.FromSeconds(30);
        _scheduler = (schedulerFactory ?? (bus => new InMemoryMessageScheduler(bus)))(this);
    }

    /// <inheritdoc />
    public Uri Address { get; }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        foreach (var host in _hosts)
        {
            await host.StartAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var host in _hosts)
        {
            await host.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        => PublishInternalAsync(message, null, cancellationToken);

    /// <inheritdoc />
    public Task<ISendEndpoint> GetSendEndpointAsync(Uri address, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(address);
        ISendEndpoint endpoint = new TransponderSendEndpoint(this, address);
        return Task.FromResult(endpoint);
    }

    /// <inheritdoc />
    public IRequestClient<TRequest> CreateRequestClient<TRequest>(TimeSpan? timeout = null)
        where TRequest : class, IMessage
    {
        var resolver = _requestAddressResolver
            ?? throw new InvalidOperationException("Request address resolver is not configured.");

        var address = resolver(typeof(TRequest))
            ?? throw new InvalidOperationException($"No request address configured for {typeof(TRequest).Name}.");

        var requestTimeout = timeout ?? _defaultRequestTimeout;

        return new RequestClient<TRequest>(
            this,
            _serializer,
            _hostProvider,
            address,
            requestTimeout);
    }

    /// <inheritdoc />
    public Task<IScheduledMessageHandle> ScheduleSendAsync<TMessage>(
        Uri destinationAddress,
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        => _scheduler.ScheduleSendAsync(destinationAddress, message, scheduledTime, cancellationToken);

    /// <inheritdoc />
    public Task<IScheduledMessageHandle> SchedulePublishAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        => _scheduler.SchedulePublishAsync(message, scheduledTime, cancellationToken);

    /// <inheritdoc />
    public Task<IScheduledMessageHandle> ScheduleSendAsync<TMessage>(
        Uri destinationAddress,
        TMessage message,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        => _scheduler.ScheduleSendAsync(destinationAddress, message, delay, cancellationToken);

    /// <inheritdoc />
    public Task<IScheduledMessageHandle> SchedulePublishAsync<TMessage>(
        TMessage message,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        => _scheduler.SchedulePublishAsync(message, delay, cancellationToken);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);

        if (_scheduler is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }

        await DisposeHostsAsync().ConfigureAwait(false);
    }

    internal async Task DisposeHostsAsync()
    {
        foreach (var host in _hosts)
        {
            await host.DisposeAsync().ConfigureAwait(false);
        }
    }

    internal async Task SendInternalAsync<TMessage>(
        Uri address,
        TMessage message,
        Guid? messageId,
        Guid? correlationId,
        Guid? conversationId,
        IReadOnlyDictionary<string, object?>? headers,
        CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(message);

        var host = _hostProvider.GetHost(address);
        var transport = await host.GetSendTransportAsync(address, cancellationToken).ConfigureAwait(false);
        var transportMessage = TransportMessageFactory.Create(
            message,
            _serializer,
            messageId,
            correlationId,
            conversationId,
            headers);

        await transport.SendAsync(transportMessage, cancellationToken).ConfigureAwait(false);
    }

    internal async Task PublishInternalAsync<TMessage>(
        TMessage message,
        IReadOnlyDictionary<string, object?>? headers,
        CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var host = _hostProvider.GetHost(Address);
        var transport = await host.GetPublishTransportAsync(message.GetType(), cancellationToken)
            .ConfigureAwait(false);
        var transportMessage = TransportMessageFactory.Create(message, _serializer, headers: headers);

        await transport.PublishAsync(transportMessage, cancellationToken).ConfigureAwait(false);
    }

    internal Task PublishObjectAsync(
        object message,
        IReadOnlyDictionary<string, object?>? headers,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var host = _hostProvider.GetHost(Address);
        return PublishObjectAsync(host, message, headers, cancellationToken);
    }

    private async Task PublishObjectAsync(
        ITransportHost host,
        object message,
        IReadOnlyDictionary<string, object?>? headers,
        CancellationToken cancellationToken)
    {
        var messageType = message.GetType();
        var transport = await host.GetPublishTransportAsync(messageType, cancellationToken).ConfigureAwait(false);
        var transportMessage = TransportMessageFactory.Create(message, _serializer, headers: headers);
        await transport.PublishAsync(transportMessage, cancellationToken).ConfigureAwait(false);
    }

    internal Uri CreateResponseAddress()
    {
        var builder = new UriBuilder(Address);
        var basePath = builder.Path?.TrimEnd('/') ?? string.Empty;
        builder.Path = $"{basePath}/responses/{Guid.NewGuid():N}";
        return builder.Uri;
    }
}
