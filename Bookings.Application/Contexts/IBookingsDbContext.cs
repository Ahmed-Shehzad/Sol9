using Bookings.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Bookings.Application.Contexts;

public interface IBookingsDbContext
{
    DbSet<Booking> Bookings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}