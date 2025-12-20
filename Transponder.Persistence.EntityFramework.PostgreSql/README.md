# Transponder.Persistence.EntityFramework.PostgreSql
PostgreSQL provider for Transponder EF Core persistence.

## Purpose
- Provide PostgreSQL specific DbContext and storage options.
- Register the scheduled message store for PostgreSQL.

## Key types
- `PostgreSqlTransponderDbContext`
- `PostgreSqlStorageOptions`
- `AddTransponderPostgreSqlPersistence`

## Registration
```csharp
services.AddDbContextFactory<PostgreSqlTransponderDbContext>(options =>
{
    // configure Npgsql provider
});

services.AddTransponderPostgreSqlPersistence();
```
