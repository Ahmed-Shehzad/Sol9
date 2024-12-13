using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;

namespace BuildingBlocks.Domain.Aggregates;

public abstract class AggregateRoot() : BaseEntity(Ulid.NewUlid()), IAggregateRoot
{
    #region Aggregate Events
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<IIntegrationEvent> _integrationsEvents = [];

    /// <summary>
    /// Gets a read-only collection of domain events that have occurred within this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
    {
        get => _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Gets a read-only collection of integration events that have occurred within this aggregate.
    /// </summary>
    public IReadOnlyCollection<IIntegrationEvent> IntegrationEvents
    {
        get => _integrationsEvents.AsReadOnly();
    }

    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all domain events from the aggregate.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddIntegrationEvent(IIntegrationEvent integrationsEvent) => _integrationsEvents.Add(integrationsEvent);

    /// <summary>
    /// Clears all integration events from the aggregate.
    /// </summary>
    public void ClearIntegrationEvents() => _integrationsEvents.Clear();

    /// <summary>
    /// Applies a domain or integration event to the aggregate.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to apply.</typeparam>
    /// <param name="event">The event to apply.</param>
    protected void ApplyChange<TEvent>(TEvent @event)
    {
        switch (@event)
        {
            case IDomainEvent domainEvent:
                AddDomainEvent(domainEvent);
                break;
            case IIntegrationEvent integrationsEvent:
                AddIntegrationEvent(integrationsEvent);
                break;
        }
    }
    #endregion Aggregate Events
    
    /// <summary>
    /// Represents the version of the aggregate root. It is incremented every time the aggregate is modified.
    /// </summary>
    /// <remarks>
    /// This property is used for optimistic concurrency control. It ensures that changes to the aggregate are only applied if the current version matches the one in the database.
    /// </remarks>
    public uint RowVersion { get; private set; }
}