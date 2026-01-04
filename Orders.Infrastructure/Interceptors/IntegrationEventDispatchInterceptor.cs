using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

using Sol9.Core;
using Sol9.Core.Abstractions;

using Transponder.Abstractions;
using Transponder.Persistence;
using Transponder.Persistence.EntityFramework;
using Transponder.Persistence.EntityFramework.PostgreSql;
using Transponder.Persistence.EntityFramework.PostgreSql.Abstractions;
using Transponder.Transports.Abstractions;

namespace Orders.Infrastructure.Interceptors;

public sealed class IntegrationEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;
    private readonly IBus _bus;
    private readonly IMessageSerializer _serializer;
    private readonly IPostgreSqlStorageOptions _storageOptions;
    private List<AggregateRoot>? _pendingAggregates;
    private List<IIntegrationEvent>? _pendingEvents;
    private IDbContextTransaction? _currentTransaction;
    private bool _ownsTransaction;

    public IntegrationEventDispatchInterceptor(
        IPublisher publisher,
        IBus bus,
        IMessageSerializer serializer,
        IPostgreSqlStorageOptions storageOptions)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            StageIntegrationEventsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public async override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await StageIntegrationEventsAsync(eventData.Context, cancellationToken).ConfigureAwait(false);

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    public async override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishAndClearPendingIntegrationEventsAsync(cancellationToken).ConfigureAwait(false);
        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishAndClearPendingIntegrationEventsAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pendingAggregates = null;
        _pendingEvents = null;
        RollbackOwnedTransactionAsync(CancellationToken.None).GetAwaiter().GetResult();
        base.SaveChangesFailed(eventData);
    }

    public override async Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pendingAggregates = null;
        _pendingEvents = null;
        await RollbackOwnedTransactionAsync(cancellationToken).ConfigureAwait(false);
        await base.SaveChangesFailedAsync(eventData, cancellationToken).ConfigureAwait(false);
    }

    private async Task RollbackOwnedTransactionAsync(CancellationToken cancellationToken)
    {
        if (_currentTransaction is null || !_ownsTransaction) return;

        await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        await _currentTransaction.DisposeAsync().ConfigureAwait(false);
        _currentTransaction = null;
        _ownsTransaction = false;
    }

    private async Task StageIntegrationEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.IntegrationEvents.Count != 0)
            .ToList();

        if (aggregates.Count == 0) return;

        var integrationEvents = aggregates
            .SelectMany(aggregate => aggregate.IntegrationEvents)
            .ToList();

        var messageIds = new HashSet<Guid>();
        var outboxMessages = new List<OutboxMessage>(integrationEvents.Count);
        foreach (IIntegrationEvent integrationEvent in integrationEvents)
        {
            OutboxMessage outboxMessage = CreateOutboxMessage(integrationEvent);
            if (!messageIds.Add(outboxMessage.MessageId)) continue;
            outboxMessages.Add(outboxMessage);
        }

        await EnsureTransactionAsync(context, cancellationToken).ConfigureAwait(false);
        await PersistOutboxMessagesAsync(context, outboxMessages, cancellationToken).ConfigureAwait(false);

        _pendingAggregates = aggregates;
        _pendingEvents = integrationEvents;
    }

    private async Task PublishAndClearPendingIntegrationEventsAsync(CancellationToken cancellationToken)
    {
        if (_pendingAggregates is null || _pendingEvents is null) return;

        await CommitOwnedTransactionAsync(cancellationToken).ConfigureAwait(false);

        List<IIntegrationEvent> eventsToPublish = _pendingEvents.ToList();
        _pendingAggregates.ForEach(aggregate => aggregate.ClearIntegrationEvents());
        _pendingAggregates = null;
        _pendingEvents = null;

        Task[] publishTasks = eventsToPublish
            .Select(integrationEvent =>
            {
                Task task = _publisher.PublishAsync((dynamic)integrationEvent, cancellationToken);
                return task;
            })
            .ToArray();

        await Task.WhenAll(publishTasks).ConfigureAwait(false);
    }

    private async Task EnsureTransactionAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is not null)
        {
            _currentTransaction = context.Database.CurrentTransaction;
            _ownsTransaction = false;
            return;
        }

        _currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _ownsTransaction = true;
    }

    private async Task PersistOutboxMessagesAsync(
        DbContext context,
        IReadOnlyCollection<OutboxMessage> messages,
        CancellationToken cancellationToken)
    {
        if (messages.Count == 0) return;

        await using PostgreSqlTransponderDbContext transponderDb = CreateTransponderDbContext(context);
        if (_currentTransaction is not null)
            _ = transponderDb.Database.UseTransaction(_currentTransaction.GetDbTransaction());

        var outboxStore = new EntityFrameworkOutboxStore(transponderDb);
        foreach (OutboxMessage message in messages)
            await outboxStore.AddAsync(message, cancellationToken).ConfigureAwait(false);

        _ = await transponderDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private PostgreSqlTransponderDbContext CreateTransponderDbContext(DbContext context)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlTransponderDbContext>();
        _ = optionsBuilder.UseNpgsql(context.Database.GetDbConnection());
        return new PostgreSqlTransponderDbContext(optionsBuilder.Options, _storageOptions);
    }

    private async Task CommitOwnedTransactionAsync(CancellationToken cancellationToken)
    {
        if (_currentTransaction is null || !_ownsTransaction) return;

        await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        await _currentTransaction.DisposeAsync().ConfigureAwait(false);
        _currentTransaction = null;
        _ownsTransaction = false;
    }

    private OutboxMessage CreateOutboxMessage(IIntegrationEvent integrationEvent)
    {
        Type messageType = integrationEvent.GetType();
        Guid messageId = integrationEvent is IntegrationEvent integration
            ? integration.EventId
            : Guid.NewGuid();
        Guid? correlationId = integrationEvent is ICorrelatedMessage correlatedMessage
            ? correlatedMessage.CorrelationId
            : null;

        ReadOnlyMemory<byte> body = _serializer.Serialize(integrationEvent, messageType);
        return new OutboxMessage(
            messageId,
            body,
            new OutboxMessageOptions
            {
                CorrelationId = correlationId,
                SourceAddress = _bus.Address,
                MessageType = messageType.FullName ?? messageType.Name,
                ContentType = _serializer.ContentType
            });
    }
}
