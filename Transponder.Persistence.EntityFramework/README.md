# Transponder.Persistence.EntityFramework
Entity Framework Core persistence implementation for Transponder.

## Purpose
- Provide EF Core implementations for inbox, outbox, scheduled message, and saga state stores.
- Define a shared `TransponderDbContext` and configurable table options.
- Offer a DbContext factory adapter for Transponder persistence services.

## Key types
- `TransponderDbContext`
- `EntityFrameworkStorageSessionFactory<TContext>`
- `EntityFrameworkInboxStore`, `EntityFrameworkOutboxStore`
- `EntityFrameworkScheduledMessageStore<TContext>`
- `EntityFrameworkSagaRepository<TState>`
- `EntityFrameworkStorageOptions`
- `EntityFrameworkDbContextFactory<TContext>`
