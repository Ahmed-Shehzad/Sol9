using Transponder.Persistence.Abstractions;

namespace Transponder.Persistence.Tests;

public sealed class InMemoryOutboxStoreTests
{
    [Fact]
    public async Task GetPendingAsync_Returns_Unsent_In_Order()
    {
        var store = new InMemoryOutboxStore();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var first = new OutboxMessage(Guid.NewGuid(), new byte[] { 1 }, enqueuedTime: now.AddMinutes(-2));
        var second = new OutboxMessage(Guid.NewGuid(), new byte[] { 2 }, enqueuedTime: now.AddMinutes(-1));
        var sent = new OutboxMessage(Guid.NewGuid(), new byte[] { 3 }, enqueuedTime: now.AddMinutes(-3), sentTime: now.AddMinutes(-1));

        await store.AddAsync(first);
        await store.AddAsync(second);
        await store.AddAsync(sent);

        IReadOnlyList<IOutboxMessage> pending = await store.GetPendingAsync(10);

        Assert.Equal([first.MessageId, second.MessageId], pending.Select(message => message.MessageId));
    }
}
