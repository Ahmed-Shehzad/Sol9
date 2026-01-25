using Microsoft.EntityFrameworkCore;

namespace Sol9.ServiceDefaults.DeadLetter;

public sealed class PostgresDeadLetterQueueDbContext : DbContext
{
    private readonly PostgresDeadLetterQueueSettings _settings;

    public PostgresDeadLetterQueueDbContext(
        DbContextOptions<PostgresDeadLetterQueueDbContext> options,
        PostgresDeadLetterQueueSettings settings)
        : base(options)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public DbSet<DeadLetterMessageEntity> DeadLetters => Set<DeadLetterMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(_settings.Schema)) _ = modelBuilder.HasDefaultSchema(_settings.Schema);

        _ = modelBuilder.Entity<DeadLetterMessageEntity>(entity =>
        {
            _ = entity.ToTable(_settings.TableName);
            _ = entity.HasKey(message => message.Id);
            _ = entity.Property(message => message.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(message => message.Reason).HasColumnName("reason");
            _ = entity.Property(message => message.Description).HasColumnName("description");
            _ = entity.Property(message => message.DlqAddress).HasColumnName("dlq_address");
            _ = entity.Property(message => message.DestinationAddress).HasColumnName("destination_address");
            _ = entity.Property(message => message.MessageType).HasColumnName("message_type");
            _ = entity.Property(message => message.MessageId).HasColumnName("message_id");
            _ = entity.Property(message => message.CorrelationId).HasColumnName("correlation_id");
            _ = entity.Property(message => message.ConversationId).HasColumnName("conversation_id");
            _ = entity.Property(message => message.ContentType).HasColumnName("content_type");
            _ = entity.Property(message => message.Headers).HasColumnName("headers").HasColumnType("jsonb");
            _ = entity.Property(message => message.Body).HasColumnName("body").HasColumnType("bytea");
            _ = entity.Property(message => message.ReplayCount).HasColumnName("replay_count");
            _ = entity.Property(message => message.LastReplayAt).HasColumnName("last_replay_at");
            _ = entity.Property(message => message.LastError).HasColumnName("last_error");
            _ = entity.HasIndex(message => message.CreatedAt)
                .HasDatabaseName($"{_settings.TableName}_created_at");
        });
    }
}

public sealed class DeadLetterMessageEntity
{
    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? Reason { get; set; }

    public string? Description { get; set; }

    public string? DlqAddress { get; set; }

    public string? DestinationAddress { get; set; }

    public string? MessageType { get; set; }

    public string? MessageId { get; set; }

    public string? CorrelationId { get; set; }

    public string? ConversationId { get; set; }

    public string? ContentType { get; set; }

    public Dictionary<string, string?>? Headers { get; set; }

    public byte[] Body { get; set; } = [];

    public int ReplayCount { get; set; }

    public DateTimeOffset? LastReplayAt { get; set; }

    public string? LastError { get; set; }
}
