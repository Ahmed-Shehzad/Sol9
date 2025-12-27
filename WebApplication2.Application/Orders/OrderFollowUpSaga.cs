using Microsoft.Extensions.Logging;
using Transponder.Abstractions;
using Transponder.Contracts.Orders;

namespace WebApplication2.Application.Orders;

public sealed class OrderFollowUpSaga :
    ISagaMessageHandler<OrderFollowUpSagaState, OrderFollowUpScheduledIntegrationEvent>
{
    private readonly ILogger<OrderFollowUpSaga> _logger;

    public OrderFollowUpSaga(ILogger<OrderFollowUpSaga> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task HandleAsync(ISagaConsumeContext<OrderFollowUpSagaState, OrderFollowUpScheduledIntegrationEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Saga.ScheduledFor = context.Message.ScheduledFor;
        context.Saga.ReceivedAt ??= DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "Received scheduled follow-up for order {OrderId} (scheduled for {ScheduledFor:o}).",
            context.Message.OrderId,
            context.Message.ScheduledFor);

        context.MarkCompleted();
        return Task.CompletedTask;
    }
}
