using BuildingBlocks.Contracts.Services.Tenants;
using BuildingBlocks.Contracts.Services.Users;
using BuildingBlocks.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Aggregates;
using Orders.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.ValueObjects;
using Orders.Infrastructure.FakeSeed;

namespace Orders.Infrastructure.Contexts;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options, ITenantService tenantService, IUserService userService)
    : BaseDbContext<OrdersDbContext>(options, tenantService.TenantId, userService.UserId)
{
    public DbSet<Order>? Orders { get; set; }
    public DbSet<OrderDocument>? OrderDocuments { get; set; }
    public DbSet<OrderItem>? OrderItems { get; set; }
    public DbSet<OrderTimeFrame>? OrderTimeFrames { get; set; }

    public static List<Order> SeedOrdersData()
    {
        var seedOrders = OrderGenerator.GenerateMinimumTenOrders();
        return seedOrders;
    }
}