using MassTransit;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Defines a handler for integration events that implements MassTransit's IConsumer interface.
/// </summary>
/// <typeparam name="TIntegrationEvent">The type of integration event to be handled. Must implement IIntegrationEvent.</typeparam>
/// <remarks>
/// This interface extends MassTransit's IConsumer interface, allowing it to be used within a MassTransit-based messaging system.
/// Implementations of this interface are responsible for processing specific types of integration events.
/// </remarks>
public interface IIntegrationEventHandler<in TIntegrationEvent> : IConsumer<TIntegrationEvent> 
    where TIntegrationEvent : class, IIntegrationEvent;