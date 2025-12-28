using System.Collections.Concurrent;
using System.Threading.Channels;

using Transponder.Persistence;
using Transponder.Persistence.Abstractions;
using Transponder.Transports;
using Transponder.Transports.Abstractions;

namespace Transponder;

public sealed class OutboxDispatcher : IAsyncDisposable
{
    private readonly IStorageSessionFactory _sessionFactory;
    private readonly ITransportHostProvider _hostProvider;
    private readonly OutboxDispatchOptions _options;
    private readonly Channel<OutboxMessage> _channel;
    private readonly ConcurrentDictionary<Guid, byte> _inflight = new();
    private readonly ConcurrentDictionary<string, byte> _activeDestinations = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _dispatchSlots;
    private CancellationTokenSource? _cts;
    private Task? _dispatchLoop;
    private Task? _pollLoop;

    public OutboxDispatcher(
        IStorageSessionFactory sessionFactory,
        ITransportHostProvider hostProvider,
        OutboxDispatchOptions options)
    {
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _hostProvider = hostProvider ?? throw new ArgumentNullException(nameof(hostProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (_options.ChannelCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(options));
        if (_options.BatchSize <= 0) throw new ArgumentOutOfRangeException(nameof(options));
        if (_options.MaxConcurrentDestinations <= 0) throw new ArgumentOutOfRangeException(nameof(options));

        var channelOptions = new BoundedChannelOptions(_options.ChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<OutboxMessage>(channelOptions);
        _dispatchSlots = new SemaphoreSlim(_options.MaxConcurrentDestinations);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_cts is not null) return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _dispatchLoop = Task.Run(() => DispatchLoopAsync(_cts.Token), _cts.Token);
        _pollLoop = Task.Run(() => PollLoopAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_cts is null) return;

        await _cts.CancelAsync().ConfigureAwait(false);
        _ = _channel.Writer.TryComplete();

        Task? dispatchLoop = _dispatchLoop;
        Task? pollLoop = _pollLoop;
        _dispatchLoop = null;
        _pollLoop = null;

        if (dispatchLoop is not null || pollLoop is not null)
        {
            var all = Task.WhenAll(dispatchLoop ?? Task.CompletedTask, pollLoop ?? Task.CompletedTask);
            _ = await Task.WhenAny(all, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }

        _activeDestinations.Clear();
        _inflight.Clear();

        _cts.Dispose();
        _cts = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _dispatchSlots.Dispose();
    }

    public async Task EnqueueAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await using IStorageSession session = await _sessionFactory.CreateSessionAsync(cancellationToken)
            .ConfigureAwait(false);
        await session.Outbox.AddAsync(message, cancellationToken).ConfigureAwait(false);
        await session.CommitAsync(cancellationToken).ConfigureAwait(false);

        TryQueue(message);
    }

    private void TryQueue(OutboxMessage message)
    {
        if (!_inflight.TryAdd(message.MessageId, 0)) return;

        if (_channel.Writer.TryWrite(message)) return;

        _ = _inflight.TryRemove(message.MessageId, out _);
    }

    private async Task DispatchLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                while (_channel.Reader.TryRead(out OutboxMessage? message))
                {
                    if (!TryGetDestinationKey(message, out string? destinationKey) || !_activeDestinations.TryAdd(destinationKey, 0))
                    {
                        _ = _inflight.TryRemove(message.MessageId, out _);
                        continue;
                    }

                    await _dispatchSlots.WaitAsync(cancellationToken).ConfigureAwait(false);

                    _ = Task.Run(() => ProcessMessageAsync(message, destinationKey, cancellationToken), cancellationToken);
                }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when cancellation is requested
        }
    }

    private async Task PollLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await PollOnceAsync(cancellationToken).ConfigureAwait(false);

            using var timer = new PeriodicTimer(_options.PollInterval);

            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                await PollOnceAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when cancellation is requested
        }
    }

    private async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        await using IStorageSession session = await _sessionFactory.CreateSessionAsync(cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<IOutboxMessage> pending = await session.Outbox
            .GetPendingAsync(_options.BatchSize, cancellationToken)
            .ConfigureAwait(false);

        foreach (IOutboxMessage message in pending)
            TryQueue(OutboxMessage.FromMessage(message));

        await session.RollbackAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        string destinationKey,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    await DispatchMessageAsync(message, cancellationToken).ConfigureAwait(false);
                    await MarkSentAsync(message.MessageId, cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch
                {
                    await Task.Delay(_options.RetryDelay, cancellationToken).ConfigureAwait(false);
                }
        }
        finally
        {
            _ = _activeDestinations.TryRemove(destinationKey, out _);
            _ = _inflight.TryRemove(message.MessageId, out _);
            _ = _dispatchSlots.Release();
        }
    }

    private async Task MarkSentAsync(Guid messageId, CancellationToken cancellationToken)
    {
        await using IStorageSession session = await _sessionFactory.CreateSessionAsync(cancellationToken)
            .ConfigureAwait(false);
        await session.Outbox.MarkSentAsync(messageId, DateTimeOffset.UtcNow, cancellationToken)
            .ConfigureAwait(false);
        await session.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task DispatchMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var transportMessage = new TransportMessage(
            message.Body,
            message.ContentType,
            message.Headers,
            message.MessageId,
            message.CorrelationId,
            message.ConversationId,
            message.MessageType,
            DateTimeOffset.UtcNow);

        if (message.DestinationAddress is not null)
        {
            ITransportHost host = _hostProvider.GetHost(message.DestinationAddress);
            ISendTransport transport = await host.GetSendTransportAsync(message.DestinationAddress, cancellationToken)
                .ConfigureAwait(false);
            await transport.SendAsync(transportMessage, cancellationToken).ConfigureAwait(false);
            return;
        }

        Type? messageType = OutboxMessageTypeResolver.Resolve(message.MessageType) ?? throw new InvalidOperationException("Outbox message type could not be resolved.");
        if (message.SourceAddress is null) throw new InvalidOperationException("Outbox publish requires a source address.");

        ITransportHost publishHost = _hostProvider.GetHost(message.SourceAddress);
        IPublishTransport publishTransport = await publishHost.GetPublishTransportAsync(messageType, cancellationToken)
            .ConfigureAwait(false);
        await publishTransport.PublishAsync(transportMessage, cancellationToken).ConfigureAwait(false);
    }

    private static bool TryGetDestinationKey(OutboxMessage message, out string destinationKey)
    {
        if (message.DestinationAddress is not null)
        {
            destinationKey = message.DestinationAddress.ToString();
            return true;
        }

        if (!string.IsNullOrWhiteSpace(message.MessageType))
        {
            destinationKey = $"publish:{message.MessageType}";
            return true;
        }

        destinationKey = string.Empty;
        return false;
    }
}
