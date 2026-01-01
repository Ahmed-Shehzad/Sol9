using Bookings.Domain.Entities;

namespace Bookings.Application.Contexts;

public interface IReadOnlyBookingsDbContext
{
    IQueryable<Booking> Bookings { get; }
}