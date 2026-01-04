using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CancelBookingResponse(
    Guid BookingId,
    Guid OrderId,
    int Status)
    : IMessage;
