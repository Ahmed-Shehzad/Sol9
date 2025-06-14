namespace Transponder.Core.Abstractions;

public interface IOutboxProcessorJob
{
    /// <summary>
    /// Processes outbox messages.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task ProcessOutboxAsync(CancellationToken cancellationToken = default);
}
