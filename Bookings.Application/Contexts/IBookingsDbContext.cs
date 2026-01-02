using Bookings.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Sol9.Core.Abstractions;

namespace Bookings.Application.Contexts;

public interface IBookingsDbContext : IDbContext
{
    DbSet<Booking> Bookings { get; }
}
