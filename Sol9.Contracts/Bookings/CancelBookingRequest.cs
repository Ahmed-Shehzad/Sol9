using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CancelBookingRequest(
    Guid OrderId,
    string CustomerName)
    : ICorrelatedMessage
{
    public Guid CorrelationId => OrderId;
}
