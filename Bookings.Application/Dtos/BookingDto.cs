namespace Bookings.Application.Dtos;

public sealed record BookingDto(
    Guid Id,
    Guid OrderId,
    string CustomerName,
    string Status,
    DateTime CreatedAtUtc);
