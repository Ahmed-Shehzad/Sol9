using Microsoft.Extensions.Hosting;

using Transponder.Transports;
using Transponder.Transports.Abstractions;

namespace Sol9.ServiceDefaults.DeadLetter;

internal sealed class PostgresDeadLetterTransportHost : TransportHostBase
{
    private readonly PostgresDeadLetterQueueStore _store;

    public PostgresDeadLetterTransportHost(
        PostgresDeadLetterQueueSettings settings,
        PostgresDeadLetterQueueStore store)
        : base(settings?.Address ?? throw new ArgumentNullException(nameof(settings)))
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        Settings = settings;
    }

    public PostgresDeadLetterQueueSettings Settings { get; }

    public override Task<ISendTransport> GetSendTransportAsync(
        Uri address,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(address);
        ISendTransport transport = new PostgresDeadLetterSendTransport(_store, address);
        return Task.FromResult(transport);
    }

    public override Task<IPublishTransport> GetPublishTransportAsync(
        Type messageType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        IPublishTransport transport = new PostgresDeadLetterPublishTransport(_store, Settings.Address);
        return Task.FromResult(transport);
    }
}

internal sealed class PostgresDeadLetterSendTransport : ISendTransport
{
    private readonly PostgresDeadLetterQueueStore _store;
    private readonly Uri _address;

    public PostgresDeadLetterSendTransport(PostgresDeadLetterQueueStore store, Uri address)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public Task SendAsync(ITransportMessage message, CancellationToken cancellationToken = default)
        => _store.InsertAsync(message, _address, cancellationToken);
}

internal sealed class PostgresDeadLetterPublishTransport : IPublishTransport
{
    private readonly PostgresDeadLetterQueueStore _store;
    private readonly Uri _address;

    public PostgresDeadLetterPublishTransport(PostgresDeadLetterQueueStore store, Uri address)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public Task PublishAsync(ITransportMessage message, CancellationToken cancellationToken = default)
        => _store.InsertAsync(message, _address, cancellationToken);
}

internal sealed class PostgresDeadLetterQueueInitializer : IHostedService
{
    private readonly PostgresDeadLetterQueueStore _store;

    public PostgresDeadLetterQueueInitializer(PostgresDeadLetterQueueStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _store.EnsureSchemaAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
