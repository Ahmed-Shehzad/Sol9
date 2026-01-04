using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CancelBookingRequest(
    Ulid OrderId,
    string CustomerName)
    : ICorrelatedMessage
{
    public Ulid CorrelationId => OrderId;
}
