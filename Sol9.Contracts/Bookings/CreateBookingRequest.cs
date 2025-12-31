using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CreateBookingRequest(
    Guid OrderId,
    string CustomerName)
    : ICorrelatedMessage
{
    public Guid CorrelationId => OrderId;
}
