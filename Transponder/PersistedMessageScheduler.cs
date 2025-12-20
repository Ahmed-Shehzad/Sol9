using Transponder.Abstractions;
using Transponder.Persistence;
using Transponder.Persistence.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder;

/// <summary>
/// Persists scheduled publish messages and dispatches them when due.
/// </summary>
public sealed class PersistedMessageScheduler : IMessageScheduler, IAsyncDisposable
{
    private readonly TransponderBus _bus;
    private readonly IScheduledMessageStore _store;
    private readonly IMessageSerializer _serializer;
    private readonly PersistedMessageSchedulerOptions _options;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    public PersistedMessageScheduler(
        TransponderBus bus,
        IScheduledMessageStore store,
        IMessageSerializer serializer,
        PersistedMessageSchedulerOptions? options = null)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _options = options ?? new PersistedMessageSchedulerOptions();

        _loop = Task.Run(() => DispatchLoopAsync(_cts.Token), _cts.Token);
    }

    public Task<IScheduledMessageHandle> ScheduleSendAsync<TMessage>(
        Uri destinationAddress,
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        => throw new NotSupportedException("Persisted scheduler only supports publish operations.");

    public Task<IScheduledMessageHandle> ScheduleSendAsync<TMessage>(
        Uri destinationAddress,
        TMessage message,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
        => throw new NotSupportedException("Persisted scheduler only supports publish operations.");

    public Task<IScheduledMessageHandle> SchedulePublishAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        if (scheduledTime <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("Scheduled time must be in the future.", nameof(scheduledTime));
        }

        return SchedulePublishInternalAsync(message, scheduledTime, cancellationToken);
    }

    public Task<IScheduledMessageHandle> SchedulePublishAsync<TMessage>(
        TMessage message,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        if (delay <= TimeSpan.Zero)
        {
            throw new ArgumentException("Delay must be greater than zero.", nameof(delay));
        }

        var scheduledTime = DateTimeOffset.UtcNow.Add(delay);
        return SchedulePublishInternalAsync(message, scheduledTime, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync().ConfigureAwait(false);

        try
        {
            await _loop.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        _cts.Dispose();
    }

    private async Task<IScheduledMessageHandle> SchedulePublishInternalAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        var messageType = message.GetType();
        var body = _serializer.Serialize(message, messageType);
        var tokenId = Guid.NewGuid();

        var stored = new ScheduledMessage(
            tokenId,
            messageType.AssemblyQualifiedName ?? messageType.FullName ?? messageType.Name,
            body,
            scheduledTime,
            contentType: _serializer.ContentType);

        await _store.AddAsync(stored, cancellationToken).ConfigureAwait(false);

        IScheduledMessageHandle handle = new PersistedScheduledMessageHandle(_store, tokenId);
        return handle;
    }

    private async Task DispatchLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await DispatchDueMessagesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await Task.Delay(_options.PollInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DispatchDueMessagesAsync(CancellationToken cancellationToken)
    {
        var due = await _store.GetDueAsync(DateTimeOffset.UtcNow, _options.BatchSize, cancellationToken)
            .ConfigureAwait(false);

        foreach (var message in due)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var messageType = ResolveMessageType(message.MessageType);
            if (messageType is null)
            {
                continue;
            }

            object? payload;

            try
            {
                payload = _serializer.Deserialize(message.Body.Span, messageType);
            }
            catch
            {
                continue;
            }

            if (payload is null)
            {
                continue;
            }

            try
            {
                await _bus.PublishObjectAsync(payload, message.Headers, cancellationToken)
                    .ConfigureAwait(false);
                await _store.MarkDispatchedAsync(message.TokenId, DateTimeOffset.UtcNow, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                // Leave for retry on the next polling cycle.
            }
        }
    }

    private static Type? ResolveMessageType(string messageType)
    {
        if (string.IsNullOrWhiteSpace(messageType))
        {
            return null;
        }

        return Type.GetType(messageType, throwOnError: false);
    }

    private sealed class PersistedScheduledMessageHandle : IScheduledMessageHandle
    {
        private readonly IScheduledMessageStore _store;

        public PersistedScheduledMessageHandle(IScheduledMessageStore store, Guid tokenId)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            if (tokenId == Guid.Empty)
            {
                throw new ArgumentException("TokenId must be provided.", nameof(tokenId));
            }

            TokenId = tokenId;
        }

        public Guid TokenId { get; }

        public Task CancelAsync(CancellationToken cancellationToken = default)
            => _store.CancelAsync(TokenId, cancellationToken);
    }
}
