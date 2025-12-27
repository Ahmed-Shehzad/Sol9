using Transponder.Contracts.Orders;

namespace WebApplication1.Application.Integration;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(OrderCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
