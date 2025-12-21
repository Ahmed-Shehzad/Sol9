using Microsoft.EntityFrameworkCore;
using Transponder.Persistence.EntityFramework.PostgreSql.Abstractions;

namespace Transponder.Persistence.EntityFramework.PostgreSql;

/// <summary>
/// PostgreSQL DbContext for Transponder inbox/outbox persistence.
/// </summary>
public sealed class PostgreSqlTransponderDbContext : TransponderDbContext
{
    public PostgreSqlTransponderDbContext(
        DbContextOptions<PostgreSqlTransponderDbContext> options,
        IPostgreSqlStorageOptions storageOptions)
        : base(options, storageOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.Property(message => message.Body).HasColumnType("bytea");
            entity.Property(message => message.Headers).HasColumnType("jsonb");
            entity.Property(message => message.SourceAddress).HasColumnType("text");
            entity.Property(message => message.DestinationAddress).HasColumnType("text");
        });

        modelBuilder.Entity<ScheduledMessageEntity>(entity =>
        {
            entity.Property(message => message.Body).HasColumnType("bytea");
            entity.Property(message => message.Headers).HasColumnType("jsonb");
            entity.Property(message => message.MessageType).HasColumnType("text");
        });

        modelBuilder.Entity<SagaStateEntity>(entity =>
        {
            entity.Property(state => state.StateType).HasColumnType("text");
            entity.Property(state => state.StateData).HasColumnType("jsonb");
        });
    }
}
