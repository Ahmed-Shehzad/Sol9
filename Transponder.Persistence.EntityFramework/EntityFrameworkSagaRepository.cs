using Microsoft.EntityFrameworkCore;
using Transponder.Persistence.Abstractions;

namespace Transponder.Persistence.EntityFramework;

/// <summary>
/// Entity Framework saga repository implementation.
/// </summary>
public sealed class EntityFrameworkSagaRepository<TState> : ISagaRepository<TState>
    where TState : class, ISagaState
{
    private readonly DbContext _context;
    private readonly string _stateType;

    public EntityFrameworkSagaRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _stateType = typeof(TState).FullName ?? typeof(TState).Name;
    }

    /// <inheritdoc />
    public async Task<TState?> GetAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        SagaStateEntity? entity = await _context.Set<SagaStateEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                state => state.CorrelationId == correlationId && state.StateType == _stateType,
                cancellationToken)
            .ConfigureAwait(false);

        return entity?.ToState<TState>();
    }

    /// <inheritdoc />
    public async Task SaveAsync(TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.CorrelationId == Guid.Empty)
        {
            throw new ArgumentException("CorrelationId must be provided.", nameof(state));
        }

        SagaStateEntity? existing = await _context.Set<SagaStateEntity>()
            .FirstOrDefaultAsync(
                entity => entity.CorrelationId == state.CorrelationId && entity.StateType == _stateType,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            var entity = SagaStateEntity.FromState(state);
            await _context.Set<SagaStateEntity>()
                .AddAsync(entity, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        var updated = SagaStateEntity.FromState(state);
        existing.ConversationId = updated.ConversationId;
        existing.StateData = updated.StateData;
        existing.UpdatedTime = updated.UpdatedTime;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        SagaStateEntity? entity = await _context.Set<SagaStateEntity>()
            .FirstOrDefaultAsync(
                state => state.CorrelationId == correlationId && state.StateType == _stateType,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return;
        }

        _context.Set<SagaStateEntity>().Remove(entity);
    }
}
