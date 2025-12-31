using Microsoft.EntityFrameworkCore;

using Npgsql;

using Orders.Domain.Entities;
using Orders.Infrastructure.Contexts;

using Testcontainers.PostgreSql;

using Xunit;

namespace Orders.IntegrationTests;

public sealed class PostgreSqlContainerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("orders")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task Container_allows_connections()
    {
        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT 1", connection);
        object? result = await command.ExecuteScalarAsync();

        Assert.Equal(1, Convert.ToInt32(result));
    }

    [Fact]
    public async Task OrdersDbContext_can_persist_order()
    {
        DbContextOptions<OrdersDbContext> options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var context = new OrdersDbContext(options);
        await context.Database.MigrateAsync();

        var order = Order.Create("Test Customer", 42.5m);
        _ = context.Orders.Add(order);
        _ = await context.SaveChangesAsync();

        Order? stored = await context.Orders.SingleOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(stored);
        Assert.Equal("Created", stored!.Status);
    }
}
