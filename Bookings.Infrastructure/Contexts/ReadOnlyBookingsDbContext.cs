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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        _ = configurationBuilder.Properties<Ulid>()
            .HaveConversion<UlidToStringConverter>()
            .HaveMaxLength(26)
            .HaveColumnType("character(26)");
        _ = configurationBuilder.Properties<Ulid?>()
            .HaveConversion<NullableUlidToStringConverter>()
            .HaveMaxLength(26)
            .HaveColumnType("character(26)");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsInfrastructure).Assembly);
    }
}
