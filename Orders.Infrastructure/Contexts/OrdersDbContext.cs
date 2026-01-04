using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Contexts;

public class OrdersDbContext : DbContext, IOrdersDbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
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
