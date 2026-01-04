using Bogus;

using Microsoft.EntityFrameworkCore;

using Orders.Application.Commands.CreateOrder;
using Orders.Application.Contexts;
using Orders.Domain.Entities;
using Orders.Infrastructure.Contexts;
using Orders.Infrastructure.Repositories;

using Shouldly;

using Testcontainers.PostgreSql;

using Xunit;

namespace Orders.IntegrationTests;

public sealed class PostgreSqlOrdersTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17.6")
        .WithDatabase("orders")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task OrdersDbContext_can_persist_orderAsync()
    {
        DbContextOptions<OrdersDbContext> options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var context = new OrdersDbContext(options);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            await context.Database.MigrateAsync();

        var order = Order.Create("Test Customer", 42.5m);
        _ = context.Orders.Add(order);
        _ = await context.SaveChangesAsync();

        Order? stored = await context.Orders.SingleOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(stored);
        Assert.Equal(OrderStatus.Created, stored.Status);
    }

    [Fact]
    public async Task OrdersDbContext_can_persist_multiple_ordersAsync()
    {
        List<Order> orders = new Faker<Order>()
            .CustomInstantiator(f => Order.Create(f.Name.FullName(), f.Finance.Amount(1, 1000, 2)))
            .Generate(3);

        DbContextOptions<OrdersDbContext> options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var context = new OrdersDbContext(options);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            await context.Database.MigrateAsync();

        await context.Orders.AddRangeAsync(orders);
        _ = await context.SaveChangesAsync();

        var ids = orders.Select(o => o.Id).ToList();
        List<Order> stored = await context.Orders
            .Where(o => ids.Contains(o.Id))
            .ToListAsync();

        stored.Count.ShouldBe(orders.Count);
        stored.Select(o => o.Id).ShouldBe(ids, ignoreOrder: true);
        stored.All(o => o.Status == OrderStatus.Created).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateOrderCommandHandler_creates_orderAsync()
    {
        var faker = new Faker();
        string customerName = faker.Name.FullName();
        decimal totalAmount = faker.Finance.Amount(10, 250, 2);

        DbContextOptions<OrdersDbContext> options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var context = new OrdersDbContext(options);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            await context.Database.MigrateAsync();

        IOrdersRepository repository = new OrdersRepository(context);
        var handler = new CreateOrderCommandHandler(repository);

        Ulid orderId = await handler.HandleAsync(new CreateOrderCommand(customerName, totalAmount), CancellationToken.None);

        Order? stored = await context.Orders.SingleOrDefaultAsync(o => o.Id == orderId);
        _ = stored.ShouldNotBeNull();
        stored.CustomerName.ShouldBe(customerName);
        stored.TotalAmount.ShouldBe(totalAmount);
        stored.Status.ShouldBe(OrderStatus.Created);
    }
}
