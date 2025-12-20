using Transponder.Persistence.EntityFramework.SqlServer.Abstractions;

namespace Transponder.Persistence.EntityFramework.SqlServer;

/// <summary>
/// Default SQL Server storage options.
/// </summary>
public sealed class SqlServerStorageOptions : EntityFrameworkStorageOptions, ISqlServerStorageOptions
{
    public SqlServerStorageOptions(
        string? schema = "dbo",
        string outboxTableName = "OutboxMessages",
        string inboxTableName = "InboxStates",
        string scheduledMessagesTableName = "ScheduledMessages",
        bool useSnapshotIsolation = true,
        string? outboxLockHint = null)
        : base(schema, outboxTableName, inboxTableName, scheduledMessagesTableName)
    {
        UseSnapshotIsolation = useSnapshotIsolation;
        OutboxLockHint = outboxLockHint;
    }

    /// <inheritdoc />
    public bool UseSnapshotIsolation { get; }

    /// <inheritdoc />
    public string? OutboxLockHint { get; }
}
