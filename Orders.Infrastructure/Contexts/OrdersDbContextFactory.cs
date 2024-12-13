using BuildingBlocks.Infrastructure.Contexts;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;

namespace Orders.Infrastructure.Contexts;

public class OrdersDbContextFactory() : DesignTimeDbContextFactory<OrdersDbContext>
{
    protected override OrdersDbContext CreateNewInstance(DbContextOptions<OrdersDbContext> options)
    {
        return new OrdersDbContext(options, null, null);
    }
    protected override void ConfigureOptions(string connectionString, DbContextOptionsBuilder<OrdersDbContext> optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(GetType().Assembly.ToString());
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextExtensions.GetDefaultSchema<OrdersDbContext>());
            sqlOptions.UseNetTopologySuite();
        });
    }
}