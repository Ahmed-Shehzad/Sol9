using BuildingBlocks.Contracts.Services.Tenants;
using BuildingBlocks.Contracts.Services.Users;
using BuildingBlocks.Infrastructure.Contexts;
using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Orders.Infrastructure.Contexts;

public class OrdersDbContextFactory() : DesignTimeDbContextFactory<OrdersDbContext>
{
    protected override OrdersDbContext CreateNewInstance(DbContextOptions<OrdersDbContext> options)
    {
        return new OrdersDbContext(options, new TenantService(), new UserService());
    }
    protected override void ConfigureOptions(string connectionString, DbContextOptionsBuilder<OrdersDbContext> optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), Array.Empty<string>());
            sqlOptions.MigrationsAssembly(GetType().Assembly.ToString());
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextExtensions.GetDefaultSchema<OrdersDbContext>());
            sqlOptions.UseNetTopologySuite();
        });
    }
}