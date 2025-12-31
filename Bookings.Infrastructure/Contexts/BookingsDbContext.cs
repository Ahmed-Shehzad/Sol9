using Bookings.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Contexts;

public class BookingsDbContext : DbContext
{
    public DbSet<Booking> Bookings => Set<Booking>();

    public BookingsDbContext(DbContextOptions<BookingsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsInfrastructure).Assembly);
    }
}
