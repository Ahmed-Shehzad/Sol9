using BuildingBlocks.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Aggregates;
using Orders.Domain.Aggregates.Entities;

namespace Orders.Infrastructure.Contexts;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options, Ulid? tenantId, Ulid? userId)
    : BaseDbContext<OrdersDbContext>(options, tenantId, userId)
{
    public DbSet<Order>? Orders { get; set; }
    public DbSet<OrderDocument>? OrderDocuments { get; set; }
    public DbSet<OrderItem>? OrderItems { get; set; }
}