using MediatR;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Defines a handler for domain events that implements MediatR's INotificationHandler.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to be handled.</typeparam>
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> 
    where TDomainEvent : IDomainEvent;