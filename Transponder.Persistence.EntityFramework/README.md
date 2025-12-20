# Transponder.Persistence.EntityFramework
Entity Framework Core persistence implementation for Transponder.

## Purpose
- Provide EF Core implementations for inbox, outbox, and scheduled message stores.
- Define a shared `TransponderDbContext` and configurable table options.
- Offer a DbContext factory adapter for Transponder persistence services.

## Key types
- `TransponderDbContext`
- `EntityFrameworkStorageSessionFactory<TContext>`
- `EntityFrameworkInboxStore`, `EntityFrameworkOutboxStore`
- `EntityFrameworkScheduledMessageStore<TContext>`
- `EntityFrameworkStorageOptions`
- `EntityFrameworkDbContextFactory<TContext>`
