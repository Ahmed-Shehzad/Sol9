# Transponder.Persistence.EntityFramework.SqlServer
SQL Server provider for Transponder EF Core persistence.

## Purpose
- Provide SQL Server specific DbContext and storage options.
- Register the scheduled message store for SQL Server.

## Key types
- `SqlServerTransponderDbContext`
- `SqlServerStorageOptions`
- `AddTransponderSqlServerPersistence`

## Registration
```csharp
services.AddDbContextFactory<SqlServerTransponderDbContext>(options =>
{
    // configure SQL Server provider
});

services.AddTransponderSqlServerPersistence();
```
