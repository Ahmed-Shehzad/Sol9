using Microsoft.EntityFrameworkCore;
using Transponder.Storage.EntityFrameworkCore.Contexts;
using Transponder.Storage.Outbox;

namespace Transponder.Storage.EntityFrameworkCore;

public class OutboxService : IOutboxService
{
    private readonly TransponderDbContext _dbContext;
    
    public OutboxService(TransponderDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .Where(x => !x.PublishedDateUtcAt.HasValue && !x.PublishedTimeUtcAt.HasValue)
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<OutboxMessage>> GetFailedMessagesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .Where(x => !string.IsNullOrWhiteSpace(x.Error))
            .ToListAsync(cancellationToken);
    }
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task AddAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.AddRange(messages);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task MarkAsProcessedAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.UpdateRange(messages);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}