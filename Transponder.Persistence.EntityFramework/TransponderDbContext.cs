using Microsoft.EntityFrameworkCore;
using Transponder.Persistence.EntityFramework.Abstractions;

namespace Transponder.Persistence.EntityFramework;

/// <summary>
/// Base DbContext for Transponder inbox/outbox persistence.
/// </summary>
public class TransponderDbContext : DbContext
{
    private readonly IEntityFrameworkStorageOptions _storageOptions;

    public TransponderDbContext(DbContextOptions options, IEntityFrameworkStorageOptions storageOptions)
        : base(options)
    {
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
    }

    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    public DbSet<InboxStateEntity> InboxStates => Set<InboxStateEntity>();

    public DbSet<ScheduledMessageEntity> ScheduledMessages => Set<ScheduledMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var schema = _storageOptions.Schema;

        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.ToTable(_storageOptions.OutboxTableName, schema);
            entity.HasKey(message => message.MessageId);
            entity.Property(message => message.MessageId).ValueGeneratedNever();
            entity.Property(message => message.Body).IsRequired();
            entity.Property(message => message.Headers);
            entity.Property(message => message.SourceAddress).HasMaxLength(2048);
            entity.Property(message => message.DestinationAddress).HasMaxLength(2048);
        });

        modelBuilder.Entity<InboxStateEntity>(entity =>
        {
            entity.ToTable(_storageOptions.InboxTableName, schema);
            entity.HasKey(state => new { state.MessageId, state.ConsumerId });
            entity.Property(state => state.MessageId).ValueGeneratedNever();
            entity.Property(state => state.ConsumerId).HasMaxLength(200);
        });

        modelBuilder.Entity<ScheduledMessageEntity>(entity =>
        {
            entity.ToTable(_storageOptions.ScheduledMessagesTableName, schema);
            entity.HasKey(message => message.TokenId);
            entity.Property(message => message.TokenId).ValueGeneratedNever();
            entity.Property(message => message.MessageType).HasMaxLength(500).IsRequired();
            entity.Property(message => message.Body).IsRequired();
            entity.Property(message => message.Headers);
            entity.HasIndex(message => new { message.ScheduledTime, message.DispatchedTime });
        });
    }
}
