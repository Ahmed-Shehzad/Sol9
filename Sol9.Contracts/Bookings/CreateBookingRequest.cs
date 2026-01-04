using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CreateBookingRequest(
    Ulid OrderId,
    string CustomerName)
    : ICorrelatedMessage
{
    public Ulid CorrelationId => OrderId;
}
