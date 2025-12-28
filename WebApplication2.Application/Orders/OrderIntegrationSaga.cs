using Intercessor.Abstractions;

using Transponder.Abstractions;
using Transponder.Contracts.Orders;

namespace WebApplication2.Application.Orders;

public sealed class OrderIntegrationSaga :
    ISagaMessageHandler<OrderIntegrationSagaState, OrderCreatedIntegrationEvent>
{
    private readonly ISender _sender;

    public OrderIntegrationSaga(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async Task HandleAsync(ISagaConsumeContext<OrderIntegrationSagaState, OrderCreatedIntegrationEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Saga.ReceivedAt ??= DateTimeOffset.UtcNow;
        var command = new ApplyOrderCreatedCommand(
            context.Message.OrderId,
            context.Message.CustomerName,
            context.Message.Total,
            context.Message.CreatedAt);
        await _sender.SendAsync(command, context.CancellationToken).ConfigureAwait(false);
        context.MarkCompleted();
    }
}
