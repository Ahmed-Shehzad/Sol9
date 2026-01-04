using Transponder.Abstractions;

namespace Sol9.Contracts.Bookings;

public sealed record CreateBookingResponse(
    Ulid BookingId,
    Ulid OrderId,
    int Status)
    : IMessage;
