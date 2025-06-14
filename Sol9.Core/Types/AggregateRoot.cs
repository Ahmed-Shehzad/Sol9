using Transponder.Core.Abstractions;

namespace Sol9.Core.Types;

public interface IAggregateRoot;

public class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents;
    private readonly List<IIntegrationEvent> _integrationEvents;
    
    public AggregateRoot(TId id) : base(id)
    {
        _domainEvents = [];
        _integrationEvents = [];
    }
    
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public IReadOnlyList<IIntegrationEvent> IntegrationEvents => _integrationEvents.AsReadOnly();
    
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    public void AddIntegrationEvent(IIntegrationEvent integrationEvent)
    {
        _integrationEvents.Add(integrationEvent);
    }
    
    public void ClearIntegrationEvents()
    {
        _integrationEvents.Clear();
    }
}