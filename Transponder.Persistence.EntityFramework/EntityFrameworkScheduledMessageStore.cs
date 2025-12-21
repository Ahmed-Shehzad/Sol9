using Microsoft.EntityFrameworkCore;
using Transponder.Persistence.Abstractions;
using Transponder.Persistence.EntityFramework.Abstractions;

namespace Transponder.Persistence.EntityFramework;

/// <summary>
/// Entity Framework scheduled message store implementation.
/// </summary>
public sealed class EntityFrameworkScheduledMessageStore<TContext> : IScheduledMessageStore
    where TContext : DbContext
{
    private readonly IEntityFrameworkDbContextFactory<TContext> _contextFactory;

    public EntityFrameworkScheduledMessageStore(IEntityFrameworkDbContextFactory<TContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task AddAsync(IScheduledMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await using TContext context = _contextFactory.CreateDbContext();
        var entity = ScheduledMessageEntity.FromMessage(message);
        await context.Set<ScheduledMessageEntity>()
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<IScheduledMessage>> GetDueAsync(
        DateTimeOffset now,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        if (maxCount <= 0) return [];

        await using TContext context = _contextFactory.CreateDbContext();
        List<ScheduledMessageEntity> messages = await context.Set<ScheduledMessageEntity>()
            .AsNoTracking()
            .Where(message => message.DispatchedTime == null && message.ScheduledTime <= now)
            .OrderBy(message => message.ScheduledTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return messages.Select(static message => (IScheduledMessage)message).ToList();
    }

    public async Task MarkDispatchedAsync(
        Guid tokenId,
        DateTimeOffset dispatchedTime,
        CancellationToken cancellationToken = default)
    {
        await using TContext context = _contextFactory.CreateDbContext();
        ScheduledMessageEntity? message = await context.Set<ScheduledMessageEntity>()
            .FirstOrDefaultAsync(entity => entity.TokenId == tokenId, cancellationToken)
            .ConfigureAwait(false);

        if (message == null) return;

        message.DispatchedTime = dispatchedTime;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CancelAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        await using TContext context = _contextFactory.CreateDbContext();
        ScheduledMessageEntity? message = await context.Set<ScheduledMessageEntity>()
            .FirstOrDefaultAsync(entity => entity.TokenId == tokenId, cancellationToken)
            .ConfigureAwait(false);

        if (message == null) return false;

        context.Set<ScheduledMessageEntity>().Remove(message);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IScheduledMessage?> GetAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        await using TContext context = _contextFactory.CreateDbContext();
        return await context.Set<ScheduledMessageEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.TokenId == tokenId, cancellationToken)
            .ConfigureAwait(false);
    }
}
