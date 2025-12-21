# Transponder.Persistence
Core persistence abstractions for inbox, outbox, and scheduled messages.

## Purpose
- Provide contracts for inbox/outbox storage and storage sessions.
- Supply in-memory store implementations for testing and development.
- Define scheduled message persistence contracts.
- Provide saga state persistence contracts and in-memory repository.

## Key types
- `IStorageSession`, `IStorageSessionFactory`
- `IInboxStore`, `IInboxState`
- `IOutboxStore`, `IOutboxMessage`
- `IScheduledMessageStore`, `IScheduledMessage`
- `ISagaState`, `ISagaRepository<TState>`
- `InMemoryInboxStore`, `InMemoryOutboxStore`, `InMemoryScheduledMessageStore`
- `InMemorySagaRepository<TState>`
