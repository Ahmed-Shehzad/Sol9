# Transponder.Persistence
Core persistence abstractions for inbox, outbox, and scheduled messages.

## Purpose
- Provide contracts for inbox/outbox storage and storage sessions.
- Supply in-memory store implementations for testing and development.
- Define scheduled message persistence contracts.

## Key types
- `IStorageSession`, `IStorageSessionFactory`
- `IInboxStore`, `IInboxState`
- `IOutboxStore`, `IOutboxMessage`
- `IScheduledMessageStore`, `IScheduledMessage`
- `InMemoryInboxStore`, `InMemoryOutboxStore`, `InMemoryScheduledMessageStore`
