using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Orders.Infrastructure.Contexts;

public sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Orders")
            ?? Environment.GetEnvironmentVariable("ORDERS_DB_CONNECTION")
            ?? "Host=localhost;Database=orders;Username=postgres;Password=postgres";

        _ = optionsBuilder.UseNpgsql(connectionString);
        return new OrdersDbContext(optionsBuilder.Options);
    }
}
