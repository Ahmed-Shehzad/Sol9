using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Transponder;
using Transponder.Transports;
using Transponder.Transports.Abstractions;

namespace Sol9.ServiceDefaults.DeadLetter;

public sealed class PostgresDeadLetterQueueStore
{
    private readonly PostgresDeadLetterQueueSettings _settings;
    private readonly IDbContextFactory<PostgresDeadLetterQueueDbContext> _dbContextFactory;

    public PostgresDeadLetterQueueStore(
        PostgresDeadLetterQueueSettings settings,
        IDbContextFactory<PostgresDeadLetterQueueDbContext> dbContextFactory)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        await using PostgresDeadLetterQueueDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(_settings.Schema))
        {
            string schemaSql = $"CREATE SCHEMA IF NOT EXISTS {QuoteIdentifier(_settings.Schema)};";
            _ = await dbContext.Database.ExecuteSqlRawAsync(schemaSql, cancellationToken).ConfigureAwait(false);
        }

        _ = await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<long> InsertAsync(
        ITransportMessage message,
        Uri dlqAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(dlqAddress);

        string? reason = GetHeaderValue(message.Headers, "DeadLetterReason");
        string? description = GetHeaderValue(message.Headers, "DeadLetterDescription");
        string? destination = GetHeaderValue(message.Headers, TransponderMessageHeaders.DestinationAddress);
        Dictionary<string, string?> headers = NormalizeHeaders(message.Headers);

        await using PostgresDeadLetterQueueDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entity = new DeadLetterMessageEntity
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Reason = reason,
            Description = description,
            DlqAddress = dlqAddress.ToString(),
            DestinationAddress = destination,
            MessageType = message.MessageType,
            MessageId = message.MessageId?.ToString(),
            CorrelationId = message.CorrelationId?.ToString(),
            ConversationId = message.ConversationId?.ToString(),
            ContentType = message.ContentType,
            Headers = headers.Count == 0 ? null : headers,
            Body = message.Body.ToArray()
        };

        _ = await dbContext.DeadLetters.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        _ = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }

    public async Task<IReadOnlyList<DeadLetterMessageSummary>> ListAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        offset = Math.Max(offset, 0);

        await using PostgresDeadLetterQueueDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await dbContext.DeadLetters
            .AsNoTracking()
            .OrderByDescending(message => message.Id)
            .Skip(offset)
            .Take(limit)
            .Select(message => new DeadLetterMessageSummary(
                message.Id,
                message.CreatedAt,
                message.Reason,
                message.DestinationAddress,
                message.MessageType,
                message.MessageId,
                message.ReplayCount,
                message.LastReplayAt,
                message.LastError))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<DeadLetterMessageDetail?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        await using PostgresDeadLetterQueueDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DeadLetterMessageEntity? entity = await dbContext.DeadLetters
            .AsNoTracking()
            .FirstOrDefaultAsync(message => message.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null) return null;

        IReadOnlyDictionary<string, string?> headers =
            entity.Headers ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        return new DeadLetterMessageDetail(
            entity.Id,
            entity.CreatedAt,
            entity.Reason,
            entity.Description,
            entity.DlqAddress,
            entity.DestinationAddress,
            entity.MessageType,
            entity.MessageId,
            entity.CorrelationId,
            entity.ConversationId,
            entity.ContentType,
            headers,
            entity.Body,
            entity.ReplayCount,
            entity.LastReplayAt,
            entity.LastError);
    }

    public async Task MarkReplayAsync(
        long id,
        bool success,
        string? error,
        CancellationToken cancellationToken = default)
    {
        await using PostgresDeadLetterQueueDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DeadLetterMessageEntity? entity = await dbContext.DeadLetters
            .FirstOrDefaultAsync(message => message.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null) return;

        entity.ReplayCount += 1;
        entity.LastReplayAt = DateTimeOffset.UtcNow;
        entity.LastError = success ? null : error ?? "Replay failed";

        _ = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Dictionary<string, string?> NormalizeHeaders(IReadOnlyDictionary<string, object?> headers)
    {
        var normalized = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, object? value) in headers) normalized[key] = CoerceToString(value);

        return normalized;
    }

    private static string? GetHeaderValue(IReadOnlyDictionary<string, object?> headers, string key)
        => headers.TryGetValue(key, out object? value) ? CoerceToString(value) : null;

    private static string? CoerceToString(object? value)
    {
        return value switch
        {
            null => null,
            string text => text,
            Uri uri => uri.ToString(),
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            JsonElement element => element.ToString(),
            _ => value.ToString()
        };
    }

    private static string QuoteIdentifier(string identifier)
        => "\"" + identifier.Replace("\"", "\"\"") + "\"";
}

public sealed record DeadLetterMessageSummary(
    long Id,
    DateTimeOffset CreatedAt,
    string? Reason,
    string? DestinationAddress,
    string? MessageType,
    string? MessageId,
    int ReplayCount,
    DateTimeOffset? LastReplayAt,
    string? LastError);

public sealed record DeadLetterMessageDetail(
    long Id,
    DateTimeOffset CreatedAt,
    string? Reason,
    string? Description,
    string? DlqAddress,
    string? DestinationAddress,
    string? MessageType,
    string? MessageId,
    string? CorrelationId,
    string? ConversationId,
    string? ContentType,
    IReadOnlyDictionary<string, string?> Headers,
    byte[] Body,
    int ReplayCount,
    DateTimeOffset? LastReplayAt,
    string? LastError)
{
    public string BodyBase64 => Convert.ToBase64String(Body);

    public TransportMessage ToTransportMessage()
    {
        var headers = Headers.ToDictionary(
            entry => entry.Key,
            object? (entry) => entry.Value,
            StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(DestinationAddress)) _ = headers.TryAdd(TransponderMessageHeaders.DestinationAddress, DestinationAddress);

        return new TransportMessage(
            Body,
            ContentType,
            headers,
            ParseUlid(MessageId),
            ParseUlid(CorrelationId),
            ParseUlid(ConversationId),
            MessageType,
            CreatedAt);
    }

    private static Ulid? ParseUlid(string? value)
        => Ulid.TryParse(value, out Ulid parsed) ? parsed : null;
}
