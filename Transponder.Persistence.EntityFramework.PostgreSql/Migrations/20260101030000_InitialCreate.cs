using System;

using Microsoft.EntityFrameworkCore.Migrations;

using Transponder.Persistence.EntityFramework.PostgreSql;
using Transponder.Persistence.EntityFramework.PostgreSql.Abstractions;

namespace Transponder.Persistence.EntityFramework.PostgreSql.Migrations;

public partial class InitialCreate : Migration
{
    private readonly IPostgreSqlStorageOptions _storageOptions;

    public InitialCreate(IPostgreSqlStorageOptions storageOptions)
    {
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        string schema = _storageOptions.Schema ?? "public";
        string outboxTable = _storageOptions.OutboxTableName;
        string inboxTable = _storageOptions.InboxTableName;
        string scheduledTable = _storageOptions.ScheduledMessagesTableName;
        string sagaTable = _storageOptions.SagaStatesTableName;

        _ = migrationBuilder.EnsureSchema(name: schema);

        _ = migrationBuilder.CreateTable(
            name: outboxTable,
            schema: schema,
            columns: table => new
            {
                MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                SourceAddress = table.Column<string>(type: "text", nullable: true),
                DestinationAddress = table.Column<string>(type: "text", nullable: true),
                MessageType = table.Column<string>(type: "text", nullable: true),
                ContentType = table.Column<string>(type: "text", nullable: true),
                Body = table.Column<byte[]>(type: "bytea", nullable: false),
                Headers = table.Column<string>(type: "jsonb", nullable: true),
                EnqueuedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                SentTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey($"PK_{outboxTable}", x => x.MessageId);
            });

        _ = migrationBuilder.CreateTable(
            name: inboxTable,
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
                _ = table.PrimaryKey($"PK_{inboxTable}", x => new { x.MessageId, x.ConsumerId });
            });

        _ = migrationBuilder.CreateTable(
            name: scheduledTable,
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
                _ = table.PrimaryKey($"PK_{scheduledTable}", x => x.TokenId);
            });

        _ = migrationBuilder.CreateTable(
            name: sagaTable,
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
                _ = table.PrimaryKey($"PK_{sagaTable}", x => new { x.CorrelationId, x.StateType });
            });

        _ = migrationBuilder.CreateIndex(
            name: $"IX_{scheduledTable}_ScheduledTime_DispatchedTime",
            schema: schema,
            table: scheduledTable,
            columns: new[] { "ScheduledTime", "DispatchedTime" });

        _ = migrationBuilder.CreateIndex(
            name: $"IX_{sagaTable}_ConversationId",
            schema: schema,
            table: sagaTable,
            column: "ConversationId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        string schema = _storageOptions.Schema ?? "public";

        _ = migrationBuilder.DropTable(
            name: _storageOptions.ScheduledMessagesTableName,
            schema: schema);

        _ = migrationBuilder.DropTable(
            name: _storageOptions.SagaStatesTableName,
            schema: schema);

        _ = migrationBuilder.DropTable(
            name: _storageOptions.InboxTableName,
            schema: schema);

        _ = migrationBuilder.DropTable(
            name: _storageOptions.OutboxTableName,
            schema: schema);
    }
}
