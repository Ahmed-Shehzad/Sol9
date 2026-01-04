using Sol9.Core.Abstractions;

namespace Sol9.Core;

public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<IIntegrationEvent> _integrationEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public IReadOnlyCollection<IIntegrationEvent> IntegrationEvents => _integrationEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    protected void AddIntegrationEvent(IIntegrationEvent integrationEvent) => _integrationEvents.Add(integrationEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
    public void ClearIntegrationEvents() => _integrationEvents.Clear();
}
