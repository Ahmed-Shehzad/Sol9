using Bookings.Application.Contexts;
using Bookings.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Contexts;

public class ReadOnlyBookingsDbContext : DbContext, IReadOnlyBookingsDbContext
{
    public IQueryable<Booking> Bookings => BookingsDbSet.AsQueryable();

    public DbSet<Booking> BookingsDbSet => Set<Booking>();

    public ReadOnlyBookingsDbContext(DbContextOptions<ReadOnlyBookingsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsInfrastructure).Assembly);
    }
}
