using System.Text.Json;
using Transponder.Abstractions;
using Transponder.Persistence;
using Transponder.Persistence.Abstractions;
using Transponder.Transports.Abstractions;

namespace Transponder;

/// <summary>
/// Persists scheduled messages and dispatches them when due.
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
    {
        ArgumentNullException.ThrowIfNull(destinationAddress);
        ArgumentNullException.ThrowIfNull(message);

        return scheduledTime <= DateTimeOffset.UtcNow
            ? throw new ArgumentException("Scheduled time must be in the future.", nameof(scheduledTime))
            : ScheduleSendInternalAsync(destinationAddress, message, scheduledTime, cancellationToken);
    }

    public Task<IScheduledMessageHandle> ScheduleSendAsync<TMessage>(
        Uri destinationAddress,
        TMessage message,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(destinationAddress);
        ArgumentNullException.ThrowIfNull(message);

        if (delay <= TimeSpan.Zero) throw new ArgumentException("Delay must be greater than zero.", nameof(delay));

        DateTimeOffset scheduledTime = DateTimeOffset.UtcNow.Add(delay);
        return ScheduleSendInternalAsync(destinationAddress, message, scheduledTime, cancellationToken);
    }

    public Task<IScheduledMessageHandle> SchedulePublishAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        return scheduledTime <= DateTimeOffset.UtcNow ? throw new ArgumentException("Scheduled time must be in the future.", nameof(scheduledTime)) : SchedulePublishInternalAsync(message, scheduledTime, cancellationToken);
    }

    public Task<IScheduledMessageHandle> SchedulePublishAsync<TMessage>(
        TMessage message,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        if (delay <= TimeSpan.Zero) throw new ArgumentException("Delay must be greater than zero.", nameof(delay));

        DateTimeOffset scheduledTime = DateTimeOffset.UtcNow.Add(delay);
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

    private Task<IScheduledMessageHandle> SchedulePublishInternalAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken)
        where TMessage : class, IMessage
        => ScheduleInternalAsync(message, scheduledTime, null, cancellationToken);

    private Task<IScheduledMessageHandle> ScheduleSendInternalAsync<TMessage>(
        Uri destinationAddress,
        TMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            [TransponderMessageHeaders.DestinationAddress] = destinationAddress.ToString()
        };

        return ScheduleInternalAsync(message, scheduledTime, headers, cancellationToken);
    }

    private async Task<IScheduledMessageHandle> ScheduleInternalAsync<TMessage>(
        TMessage message,
        DateTimeOffset scheduledTime,
        IReadOnlyDictionary<string, object?>? headers,
        CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        Type messageType = message.GetType();
        ReadOnlyMemory<byte> body = _serializer.Serialize(message, messageType);
        var tokenId = Guid.NewGuid();

        var stored = new ScheduledMessage(
            tokenId,
            messageType.AssemblyQualifiedName ?? messageType.FullName ?? messageType.Name,
            body,
            scheduledTime,
            headers,
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
        IReadOnlyList<IScheduledMessage> due = await _store.GetDueAsync(DateTimeOffset.UtcNow, _options.BatchSize, cancellationToken)
            .ConfigureAwait(false);

        foreach (IScheduledMessage message in due)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Type? messageType = ResolveMessageType(message.MessageType);
            if (messageType is null) continue;

            object? payload;

            try
            {
                payload = _serializer.Deserialize(message.Body.Span, messageType);
            }
            catch
            {
                continue;
            }

            Uri? destinationAddress = null;
            if (message.Headers.TryGetValue(TransponderMessageHeaders.DestinationAddress, out object? destinationValue))
            {
                if (!TryParseDestinationAddress(destinationValue, out Uri parsed)) continue;

                destinationAddress = parsed;
            }

            try
            {
                if (destinationAddress is null)
                    await _bus.PublishObjectAsync(payload, message.Headers, cancellationToken)
                        .ConfigureAwait(false);
                else
                {
                    IReadOnlyDictionary<string, object?> dispatchHeaders = RemoveDestinationHeader(message.Headers);
                    await _bus.SendObjectAsync(destinationAddress, payload, dispatchHeaders, cancellationToken)
                        .ConfigureAwait(false);
                }

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
        if (string.IsNullOrWhiteSpace(messageType)) return null;

        return Type.GetType(messageType, throwOnError: false);
    }

    private static bool TryParseDestinationAddress(object? value, out Uri destinationAddress)
    {
        destinationAddress = null!;

        string? address = value switch
        {
            null => null,
            Uri uri => uri.ToString(),
            string text => text,
            JsonElement element when element.ValueKind == JsonValueKind.String => element.GetString(),
            JsonElement => null,
            _ => value.ToString()
        };

        if (string.IsNullOrWhiteSpace(address)) return false;

        return Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out destinationAddress);
    }

    private static IReadOnlyDictionary<string, object?> RemoveDestinationHeader(
        IReadOnlyDictionary<string, object?> headers)
    {
        var filtered = new Dictionary<string, object?>(headers, StringComparer.OrdinalIgnoreCase);
        filtered.Remove(TransponderMessageHeaders.DestinationAddress);
        return filtered;
    }

    private sealed class PersistedScheduledMessageHandle : IScheduledMessageHandle
    {
        private readonly IScheduledMessageStore _store;

        public PersistedScheduledMessageHandle(IScheduledMessageStore store, Guid tokenId)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            if (tokenId == Guid.Empty) throw new ArgumentException("TokenId must be provided.", nameof(tokenId));

            TokenId = tokenId;
        }

        public Guid TokenId { get; }

        public Task CancelAsync(CancellationToken cancellationToken = default)
            => _store.CancelAsync(TokenId, cancellationToken);
    }
}
