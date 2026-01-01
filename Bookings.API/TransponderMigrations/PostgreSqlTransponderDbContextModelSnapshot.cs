using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Transponder.Persistence.EntityFramework.PostgreSql;

namespace Bookings.API.TransponderMigrations;

[DbContext(typeof(PostgreSqlTransponderDbContext))]
public partial class PostgreSqlTransponderDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        const string schema = "bookings_transponder";

        modelBuilder.Entity("Transponder.Persistence.EntityFramework.InboxStateEntity", b =>
        {
            b.Property<Guid>("MessageId")
                .HasColumnType("uuid");

            b.Property<string>("ConsumerId")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<DateTimeOffset>("ReceivedTime")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("ProcessedTime")
                .HasColumnType("timestamp with time zone");

            b.HasKey("MessageId", "ConsumerId");

            b.ToTable("InboxStates", schema);
        });

        modelBuilder.Entity("Transponder.Persistence.EntityFramework.OutboxMessageEntity", b =>
        {
            b.Property<Guid>("MessageId")
                .HasColumnType("uuid");

            b.Property<Guid?>("CorrelationId")
                .HasColumnType("uuid");

            b.Property<Guid?>("ConversationId")
                .HasColumnType("uuid");

            b.Property<string>("SourceAddress")
                .HasMaxLength(2048)
                .HasColumnType("text");

            b.Property<string>("DestinationAddress")
                .HasMaxLength(2048)
                .HasColumnType("text");

            b.Property<string>("MessageType")
                .HasColumnType("text");

            b.Property<string>("ContentType")
                .HasColumnType("text");

            b.Property<byte[]>("Body")
                .IsRequired()
                .HasColumnType("bytea");

            b.Property<string>("Headers")
                .HasColumnType("jsonb");

            b.Property<DateTimeOffset>("EnqueuedTime")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("SentTime")
                .HasColumnType("timestamp with time zone");

            b.HasKey("MessageId");

            b.ToTable("OutboxMessages", schema);
        });

        modelBuilder.Entity("Transponder.Persistence.EntityFramework.SagaStateEntity", b =>
        {
            b.Property<Guid>("CorrelationId")
                .HasColumnType("uuid");

            b.Property<string>("StateType")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("text");

            b.Property<string>("StateData")
                .IsRequired()
                .HasColumnType("jsonb");

            b.Property<Guid?>("ConversationId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedTime")
                .HasColumnType("timestamp with time zone");

            b.HasKey("CorrelationId", "StateType");

            b.HasIndex("ConversationId");

            b.ToTable("SagaStates", schema);
        });

        modelBuilder.Entity("Transponder.Persistence.EntityFramework.ScheduledMessageEntity", b =>
        {
            b.Property<Guid>("TokenId")
                .HasColumnType("uuid");

            b.Property<string>("MessageType")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("text");

            b.Property<string>("ContentType")
                .HasColumnType("text");

            b.Property<byte[]>("Body")
                .IsRequired()
                .HasColumnType("bytea");

            b.Property<string>("Headers")
                .HasColumnType("jsonb");

            b.Property<DateTimeOffset>("ScheduledTime")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset>("CreatedTime")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DispatchedTime")
                .HasColumnType("timestamp with time zone");

            b.HasKey("TokenId");

            b.HasIndex("ScheduledTime", "DispatchedTime");

            b.ToTable("ScheduledMessages", schema);
        });
    }
}
