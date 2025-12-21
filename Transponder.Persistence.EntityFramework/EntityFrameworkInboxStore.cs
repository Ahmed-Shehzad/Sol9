using Microsoft.EntityFrameworkCore;
using Transponder.Persistence.Abstractions;

namespace Transponder.Persistence.EntityFramework;

/// <summary>
/// Entity Framework inbox store implementation.
/// </summary>
public sealed class EntityFrameworkInboxStore : IInboxStore
{
    private readonly DbContext _context;

    public EntityFrameworkInboxStore(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IInboxState?> GetAsync(
        Guid messageId,
        string consumerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumerId)) throw new ArgumentException("ConsumerId must be provided.", nameof(consumerId));

        return await _context.Set<InboxStateEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                state => state.MessageId == messageId && state.ConsumerId == consumerId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> TryAddAsync(IInboxState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        bool exists = await _context.Set<InboxStateEntity>()
            .AsNoTracking()
            .AnyAsync(
                existing => existing.MessageId == state.MessageId && existing.ConsumerId == state.ConsumerId,
                cancellationToken)
            .ConfigureAwait(false);

        if (exists) return false;

        var entity = InboxStateEntity.FromState(state);

        await _context.Set<InboxStateEntity>()
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task MarkProcessedAsync(
        Guid messageId,
        string consumerId,
        DateTimeOffset processedTime,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumerId)) throw new ArgumentException("ConsumerId must be provided.", nameof(consumerId));

        InboxStateEntity? state = await _context.Set<InboxStateEntity>()
            .FirstOrDefaultAsync(
                entry => entry.MessageId == messageId && entry.ConsumerId == consumerId,
                cancellationToken)
            .ConfigureAwait(false);

        if (state == null) return;

        state.ProcessedTime = processedTime;
    }
}
