using Microsoft.Extensions.DependencyInjection;

using Transponder.Transports.Abstractions;

namespace Transponder;

internal sealed class SagaReceiveEndpointGroup : IReceiveEndpoint
{
    private readonly SagaEndpointRegistry _registry;
    private readonly ITransportHostProvider _hostProvider;
    private readonly IMessageSerializer _serializer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<IReceiveEndpoint> _endpoints = [];
    private bool _started;

    public SagaReceiveEndpointGroup(
        SagaEndpointRegistry registry,
        ITransportHostProvider hostProvider,
        IMessageSerializer serializer,
        IServiceScopeFactory scopeFactory)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _hostProvider = hostProvider ?? throw new ArgumentNullException(nameof(hostProvider));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        InputAddress = new Uri("transponder://saga");
    }

    public Uri InputAddress { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_started)
        {
            return;
        }

        _started = true;
        foreach (Uri inputAddress in _registry.GetInputAddresses())
        {
            var handler = new SagaReceiveEndpointHandler(
                inputAddress,
                _registry,
                _serializer,
                _scopeFactory);

            var configuration = new ReceiveEndpointConfiguration(
                inputAddress,
                handler.HandleAsync);

            ITransportHost host = _hostProvider.GetHost(inputAddress);
            IReceiveEndpoint endpoint = host.ConnectReceiveEndpoint(configuration);
            _endpoints.Add(endpoint);
        }

        foreach (IReceiveEndpoint endpoint in _endpoints)
        {
            await endpoint.StartAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_started)
        {
            return;
        }

        foreach (IReceiveEndpoint endpoint in _endpoints)
        {
            await endpoint.StopAsync(cancellationToken).ConfigureAwait(false);
            await endpoint.DisposeAsync().ConfigureAwait(false);
        }

        _endpoints.Clear();
        _started = false;
    }

    public ValueTask DisposeAsync() => new(StopAsync());
}
