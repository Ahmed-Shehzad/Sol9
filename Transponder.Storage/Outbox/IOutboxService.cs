namespace Transponder.Storage.Outbox;

public interface IOutboxService
{
    Task<IReadOnlyList<OutboxMessage>> GetMessagesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task AddAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default);
    Task<OutboxMessage?> GetByMessageIdAsync(Ulid messageId, CancellationToken cancellationToken = default);
    Task MarkAsPublishedAsync(Ulid messageId, CancellationToken cancellationToken = default);
}