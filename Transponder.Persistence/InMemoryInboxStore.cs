using Transponder.Persistence.Abstractions;

namespace Transponder.Persistence;

/// <summary>
/// In-memory inbox store for testing and development.
/// </summary>
public sealed class InMemoryInboxStore : IInboxStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<(Guid MessageId, string ConsumerId), InboxState> _states = new();

    /// <inheritdoc />
    public Task<IInboxState?> GetAsync(
        Guid messageId,
        string consumerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
        {
            throw new ArgumentException("ConsumerId must be provided.", nameof(consumerId));
        }

        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _states.TryGetValue((messageId, consumerId), out InboxState? state);
            return Task.FromResult<IInboxState?>(state);
        }
    }

    /// <inheritdoc />
    public Task<bool> TryAddAsync(IInboxState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            (Guid MessageId, string ConsumerId) key = (state.MessageId, state.ConsumerId);

            if (_states.ContainsKey(key))
            {
                return Task.FromResult(false);
            }

            _states[key] = InboxState.FromState(state);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task MarkProcessedAsync(
        Guid messageId,
        string consumerId,
        DateTimeOffset processedTime,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
        {
            throw new ArgumentException("ConsumerId must be provided.", nameof(consumerId));
        }

        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (_states.TryGetValue((messageId, consumerId), out InboxState? state))
            {
                state.MarkProcessed(processedTime);
            }
        }

        return Task.CompletedTask;
    }
}
