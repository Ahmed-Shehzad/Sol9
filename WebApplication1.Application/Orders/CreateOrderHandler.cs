using Intercessor.Abstractions;

using Transponder.Contracts.Orders;

using WebApplication1.Application.Integration;
using WebApplication1.Domain.Orders;

namespace WebApplication1.Application.Orders;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _repository;
    private readonly IIntegrationEventPublisher _publisher;

    public CreateOrderHandler(IOrderRepository repository, IIntegrationEventPublisher publisher)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<CreateOrderResult> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var order = Order.Create(command.CustomerName, command.Total, DateTimeOffset.UtcNow);
        await _repository.SaveAsync(order, cancellationToken).ConfigureAwait(false);

        var integrationEvent = new OrderCreatedIntegrationEvent(
            order.Id,
            order.CustomerName,
            order.Total,
            order.CreatedAt);

        await _publisher.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        return new CreateOrderResult(order.Id);
    }
}
