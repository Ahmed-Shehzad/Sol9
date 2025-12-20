using Transponder.Abstractions;

namespace Transponder;

/// <summary>
/// Default scheduled message handle.
/// </summary>
public sealed class ScheduledMessageHandle : IScheduledMessageHandle
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    public ScheduledMessageHandle(Guid tokenId, CancellationTokenSource cancellationTokenSource)
    {
        if (tokenId == Guid.Empty)
        {
            throw new ArgumentException("TokenId must be provided.", nameof(tokenId));
        }

        TokenId = tokenId;
        _cancellationTokenSource = cancellationTokenSource
            ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
    }

    /// <inheritdoc />
    public Guid TokenId { get; }

    /// <inheritdoc />
    public Task CancelAsync(CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        return Task.CompletedTask;
    }
}
