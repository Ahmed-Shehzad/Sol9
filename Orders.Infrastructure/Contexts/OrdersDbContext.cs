using Microsoft.EntityFrameworkCore;

using Orders.Application.Contracts;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Contexts;

public class OrdersDbContext : DbContext, IOrdersDbContext, IReadOnlyOrdersDbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersInfrastructure).Assembly);
    }
}
