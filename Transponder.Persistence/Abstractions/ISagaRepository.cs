namespace Transponder.Persistence.Abstractions;

/// <summary>
/// Provides access to saga state storage.
/// </summary>
/// <typeparam name="TState">The saga state type.</typeparam>
public interface ISagaRepository<TState>
    where TState : class, ISagaState
{
    /// <summary>
    /// Gets a saga state by correlation identifier.
    /// </summary>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<TState?> GetAsync(Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the saga state.
    /// </summary>
    /// <param name="state">The saga state.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task SaveAsync(TState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a saga state by correlation identifier.
    /// </summary>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task DeleteAsync(Guid correlationId, CancellationToken cancellationToken = default);
}
