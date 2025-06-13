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
    
    public async Task<IReadOnlyList<OutboxMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        return _messages.AsReadOnly();
    }
    
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _messages.Add(message);
    }
    
    public async Task AddAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        _messages.AddRange(messages);
    }
    
    public async Task<OutboxMessage?> GetByMessageIdAsync(Ulid messageId, CancellationToken cancellationToken = default)
    {
        return _messages.Find(m => m.Id == messageId);
    }
    
    public async Task MarkAsPublishedAsync(Ulid messageId, CancellationToken cancellationToken = default)
    {
        var outbox = await GetByMessageIdAsync(messageId, cancellationToken);
        if (outbox is not null)
        {
            outbox.MarkAsPublished();
            _messages.Remove(outbox);
        }
    }
}