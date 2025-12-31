namespace Bookings.API.Contracts;

public sealed record CreateBookingRequest(Guid OrderId, string CustomerName);
