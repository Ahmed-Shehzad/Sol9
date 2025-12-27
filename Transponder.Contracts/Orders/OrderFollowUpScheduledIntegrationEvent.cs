using Transponder.Abstractions;

namespace Transponder.Contracts.Orders;

public sealed record OrderFollowUpScheduledIntegrationEvent(
    Guid OrderId,
    DateTimeOffset ScheduledFor) : ICorrelatedMessage
{
    public Guid CorrelationId => OrderId;
}
