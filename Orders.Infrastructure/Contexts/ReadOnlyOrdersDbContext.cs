using System.Linq;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersInfrastructure).Assembly);
    }
}
