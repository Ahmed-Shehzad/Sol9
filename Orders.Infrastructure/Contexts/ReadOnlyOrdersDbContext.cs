using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Contexts;

public class ReadOnlyOrdersDbContext : DbContext, IReadOnlyOrdersDbContext
{
    public IQueryable<Order> Orders => OrdersDbSet.AsQueryable();

    public DbSet<Order> OrdersDbSet => Set<Order>();

    public ReadOnlyOrdersDbContext(DbContextOptions<ReadOnlyOrdersDbContext> options) : base(options)
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

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersInfrastructure).Assembly);
    }
}
