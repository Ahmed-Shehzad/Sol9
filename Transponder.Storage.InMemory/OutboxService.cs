using Transponder.Core;
using Transponder.Storage.Outbox;

namespace Transponder.Storage.InMemory;

public class OutboxService : IOutboxService
{
    private readonly ConcurrentList<OutboxMessage> _messages;
    
    public OutboxService()
    {
        _messages = new ConcurrentList<OutboxMessage>();
    }
    
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default)
    {
        return _messages.Where(m => m.IsUnprocessed()).AsReadOnly();
    }
    
    public async Task<IReadOnlyList<OutboxMessage>> GetFailedMessagesAsync(CancellationToken cancellationToken = default)
    {
        return _messages.Where(m => m.IsFailed()).AsReadOnly();
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _messages.Add(message);
    }
    
    public async Task AddAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        _messages.AddRange(messages);
    }
    
    private async Task<OutboxMessage?> GetByMessageIdAsync(Ulid messageId, CancellationToken cancellationToken = default)
    {
        return _messages.Find(m => m.Id == messageId);
    }
    
    public async Task MarkAsProcessedAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        var outboxMessages = messages.ToList();
        
        var publishedMessages = outboxMessages.Where(m => m.IsPublished());
        _messages.RemoveAll(publishedMessages);
        
        var failedMessages = outboxMessages.Where(m => m.IsFailed());
        foreach (var message in failedMessages)
        {
            var storedMessage = await GetByMessageIdAsync(message.Id, cancellationToken);
            if (storedMessage == null) continue;

            _messages.Remove(storedMessage);
            _messages.Add(message);
        }
    }
}