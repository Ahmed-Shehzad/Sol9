using Microsoft.EntityFrameworkCore.Migrations;

namespace Bookings.API.TransponderMigrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        const string schema = "bookings_transponder";

        migrationBuilder.EnsureSchema(name: schema);

        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            schema: schema,
            columns: table => new
            {
                MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                SourceAddress = table.Column<string>(type: "text", maxLength: 2048, nullable: true),
                DestinationAddress = table.Column<string>(type: "text", maxLength: 2048, nullable: true),
                MessageType = table.Column<string>(type: "text", nullable: true),
                ContentType = table.Column<string>(type: "text", nullable: true),
                Body = table.Column<byte[]>(type: "bytea", nullable: false),
                Headers = table.Column<string>(type: "jsonb", nullable: true),
                EnqueuedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                SentTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutboxMessages", x => x.MessageId);
            });

        migrationBuilder.CreateTable(
            name: "InboxStates",
            schema: schema,
            columns: table => new
            {
                MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                ConsumerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ReceivedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ProcessedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InboxStates", x => new { x.MessageId, x.ConsumerId });
            });

        migrationBuilder.CreateTable(
            name: "ScheduledMessages",
            schema: schema,
            columns: table => new
            {
                TokenId = table.Column<Guid>(type: "uuid", nullable: false),
                MessageType = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                ContentType = table.Column<string>(type: "text", nullable: true),
                Body = table.Column<byte[]>(type: "bytea", nullable: false),
                Headers = table.Column<string>(type: "jsonb", nullable: true),
                ScheduledTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                DispatchedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ScheduledMessages", x => x.TokenId);
            });

        migrationBuilder.CreateTable(
            name: "SagaStates",
            schema: schema,
            columns: table => new
            {
                CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                StateType = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                StateData = table.Column<string>(type: "jsonb", nullable: false),
                ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SagaStates", x => new { x.CorrelationId, x.StateType });
            });

        migrationBuilder.CreateIndex(
            name: "IX_ScheduledMessages_ScheduledTime_DispatchedTime",
            schema: schema,
            table: "ScheduledMessages",
            columns: new[] { "ScheduledTime", "DispatchedTime" });

        migrationBuilder.CreateIndex(
            name: "IX_SagaStates_ConversationId",
            schema: schema,
            table: "SagaStates",
            column: "ConversationId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        const string schema = "bookings_transponder";

        migrationBuilder.DropTable(
            name: "ScheduledMessages",
            schema: schema);

        migrationBuilder.DropTable(
            name: "SagaStates",
            schema: schema);

        migrationBuilder.DropTable(
            name: "InboxStates",
            schema: schema);

        migrationBuilder.DropTable(
            name: "OutboxMessages",
            schema: schema);
    }
}
