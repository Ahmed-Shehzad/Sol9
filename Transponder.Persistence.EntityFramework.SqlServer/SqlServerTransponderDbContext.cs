using Microsoft.EntityFrameworkCore;
using Transponder.Persistence.EntityFramework.SqlServer.Abstractions;

namespace Transponder.Persistence.EntityFramework.SqlServer;

/// <summary>
/// SQL Server DbContext for Transponder inbox/outbox persistence.
/// </summary>
public sealed class SqlServerTransponderDbContext : TransponderDbContext
{
    public SqlServerTransponderDbContext(
        DbContextOptions<SqlServerTransponderDbContext> options,
        ISqlServerStorageOptions storageOptions)
        : base(options, storageOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.Property(message => message.Body).HasColumnType("varbinary(max)");
            entity.Property(message => message.Headers).HasColumnType("nvarchar(max)");
            entity.Property(message => message.SourceAddress).HasColumnType("nvarchar(2048)");
            entity.Property(message => message.DestinationAddress).HasColumnType("nvarchar(2048)");
        });

        modelBuilder.Entity<ScheduledMessageEntity>(entity =>
        {
            entity.Property(message => message.Body).HasColumnType("varbinary(max)");
            entity.Property(message => message.Headers).HasColumnType("nvarchar(max)");
            entity.Property(message => message.MessageType).HasColumnType("nvarchar(500)");
        });

        modelBuilder.Entity<SagaStateEntity>(entity =>
        {
            entity.Property(state => state.StateType).HasColumnType("nvarchar(500)");
            entity.Property(state => state.StateData).HasColumnType("nvarchar(max)");
        });
    }
}
