using Transponder.Abstractions;

namespace Transponder.Contracts.Orders;

public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    string CustomerName,
    decimal Total,
    DateTimeOffset CreatedAt) : ICorrelatedMessage
{
    public Guid CorrelationId => OrderId;
}
