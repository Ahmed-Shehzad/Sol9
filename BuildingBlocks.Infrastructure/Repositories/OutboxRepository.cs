using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Repositories;

public class OutboxRepository<TContext>(BaseDbContext<TContext> context) : Repository<Outbox>(context), IOutboxRepository where TContext : DbContext
{
    public async Task<List<Outbox>> FindAllProcessedMessagesAsync(CancellationToken cancellationToken = default)
    {
        if (context.OutboxMessages == null) return [];

        var messages = await context.OutboxMessages.IgnoreQueryFilters().Where(m => m.Processed).AsQueryable().ToListAsync(cancellationToken);
        return messages;
    }
}