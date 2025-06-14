namespace Transponder.Core.Abstractions;

public interface IBusPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent;
}