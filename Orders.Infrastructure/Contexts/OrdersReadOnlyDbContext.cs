using BuildingBlocks.Contracts.Services.Tenants;
using BuildingBlocks.Contracts.Services.Users;
using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Aggregates;
using Orders.Infrastructure.Contexts.Contracts;

namespace Orders.Infrastructure.Contexts;

public class OrdersReadOnlyDbContext(
    DbContextOptions<OrdersDbContext> options,
    ITenantService tenantService,
    IUserService userService)
    : OrdersDbContext(options, tenantService, userService), IOrdersReadOnlyDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var schema = DbContextExtensions.GetDefaultSchema<OrdersDbContext>();
        modelBuilder.HasDefaultSchema(schema);
    }

    IQueryable<Order> IOrdersReadOnlyDbContext.Orders
    {
        get
        {
            if (Orders is null) return Enumerable.Empty<Order>().AsQueryable();

            return Orders
                .Include(o => o.Items)
                .Include(o => o.Depots)
                .Include(o => o.Documents)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .Include(o => o.TransportAddress)
                .Include(o => o.TimeFrames)
                .AsSplitQuery()
                .AsQueryable();
        }
    }
}