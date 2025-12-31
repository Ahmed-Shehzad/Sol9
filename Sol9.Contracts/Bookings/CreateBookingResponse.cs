using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CreateBookingResponse(
    Guid BookingId,
    Guid OrderId,
    string Status)
    : IMessage;
