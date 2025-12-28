using Sol9.Eventing;

using Transponder.Abstractions;

namespace Transponder.Contracts.Orders;

public sealed record OrderFollowUpScheduledIntegrationEvent(
    Guid OrderId,
    DateTimeOffset ScheduledFor) : IntegrationEvent, ICorrelatedMessage
{
    public Guid CorrelationId => OrderId;
}
