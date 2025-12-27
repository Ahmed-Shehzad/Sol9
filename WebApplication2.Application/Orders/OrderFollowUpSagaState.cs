using Transponder.Persistence.Abstractions;

namespace WebApplication2.Application.Orders;

public sealed class OrderFollowUpSagaState : ISagaState
{
    public Guid CorrelationId { get; set; }

    public Guid? ConversationId { get; set; }

    public DateTimeOffset? ScheduledFor { get; set; }

    public DateTimeOffset? ReceivedAt { get; set; }
}
