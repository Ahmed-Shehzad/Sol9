using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CancelBookingResponse(
    Ulid BookingId,
    Ulid OrderId,
    int Status)
    : IMessage;
