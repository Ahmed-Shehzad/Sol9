using Microsoft.Extensions.Configuration;

namespace Sol9.ServiceDefaults.DeadLetter;

public sealed class PostgresDeadLetterQueueSettings
{
    public PostgresDeadLetterQueueSettings(
        string connectionString,
        Uri address,
        string tableName,
        string? schema)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        TableName = string.IsNullOrWhiteSpace(tableName)
            ? throw new ArgumentException("Table name is required.", nameof(tableName))
            : tableName;
        Schema = string.IsNullOrWhiteSpace(schema) ? null : schema;
    }

    public string ConnectionString { get; }

    public Uri Address { get; }

    public string TableName { get; }

    public string? Schema { get; }

    public static PostgresDeadLetterQueueSettings? FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? connectionString = configuration.GetConnectionString("Transponder");
        if (string.IsNullOrWhiteSpace(connectionString)) return null;

        string addressText = configuration["DeadLetterQueue:Address"] ?? "dlq://deadletter";
        if (!Uri.TryCreate(addressText, UriKind.Absolute, out Uri? address)) return null;

        string tableName = configuration["DeadLetterQueue:Table"] ?? "transponder_dead_letter";
        string? schema = configuration["DeadLetterQueue:Schema"] ?? configuration["TransponderPersistence:Schema"];

        return new PostgresDeadLetterQueueSettings(connectionString, address, tableName, schema);
    }
}
