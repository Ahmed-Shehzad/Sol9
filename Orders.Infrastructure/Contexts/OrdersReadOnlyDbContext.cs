using Microsoft.EntityFrameworkCore;
using Orders.Domain.Aggregates;
using Orders.Infrastructure.Contexts.Contracts;

namespace Orders.Infrastructure.Contexts;

public class OrdersReadOnlyDbContext(
    DbContextOptions<OrdersDbContext> options,
    Ulid? tenantId,
    Ulid? userId)
    : OrdersDbContext(options,
        tenantId,
        userId), IOrdersReadOnlyDbContext
{
    IQueryable<Order> IOrdersReadOnlyDbContext.Orders
    {
        get
        {
            if (Orders is null) return Enumerable.Empty<Order>().AsQueryable();

            return Orders
                .Include(o => o.Items)
                .Include(o => o.Depots)
                .Include(o => o.Documents)
                .AsSplitQuery()
                .AsQueryable();
        }
    }
}