using Bogus;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

using Orders.Application.Commands.CreateOrder;
using Orders.Application.Dtos.Orders;
using Orders.Domain.Entities;
using Orders.Infrastructure.Contexts;

using Shouldly;

using Sol9.Contracts.Bookings;

using Testcontainers.PostgreSql;

using Transponder.Abstractions;

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
        await context.Database.MigrateAsync();

        await context.Orders.AddRangeAsync(orders);
        _ = await context.SaveChangesAsync();

        List<Guid> ids = orders.Select(o => o.Id).ToList();
        List<Order> stored = await context.Orders
            .Where(o => ids.Contains(o.Id))
            .ToListAsync();

        stored.Count.ShouldBe(orders.Count);
        stored.Select(o => o.Id).ShouldBe(ids, ignoreOrder: true);
        stored.All(o => o.Status == OrderStatus.Created).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateOrderCommandHandler_marks_order_booked_when_booking_createdAsync()
    {
        var faker = new Faker();
        string customerName = faker.Name.FullName();
        decimal totalAmount = faker.Finance.Amount(10, 250, 2);

        DbContextOptions<OrdersDbContext> options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var context = new OrdersDbContext(options);
        await context.Database.MigrateAsync();

        IRequestClient<CreateBookingRequest> requestClient = Substitute.For<IRequestClient<CreateBookingRequest>>();
        _ = requestClient.GetResponseAsync<CreateBookingResponse>(Arg.Any<CreateBookingRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                CreateBookingRequest request = callInfo.ArgAt<CreateBookingRequest>(0);
                return Task.FromResult(new CreateBookingResponse(
                    Guid.NewGuid(),
                    request.OrderId,
                    (int)BookingStatus.Created));
            });

        IClientFactory clientFactory = Substitute.For<IClientFactory>();
        _ = clientFactory.CreateRequestClient<CreateBookingRequest>(Arg.Any<TimeSpan?>())
            .Returns(requestClient);

        var handler = new CreateOrderCommandHandler(context, clientFactory);

        OrderDto result = await handler.HandleAsync(new CreateOrderCommand(customerName, totalAmount), CancellationToken.None);

        result.CustomerName.ShouldBe(customerName);
        result.TotalAmount.ShouldBe(totalAmount);
        result.Status.ShouldBe(OrderStatus.Booked);

        Order? stored = await context.Orders.SingleOrDefaultAsync(o => o.Id == result.Id);
        _ = stored.ShouldNotBeNull();
        stored.Status.ShouldBe(OrderStatus.Booked);

        _ = clientFactory.Received(1).CreateRequestClient<CreateBookingRequest>(null);
        _ = await requestClient.Received(1).GetResponseAsync<CreateBookingResponse>(
            Arg.Is<CreateBookingRequest>(request => request.CustomerName == customerName),
            Arg.Any<CancellationToken>());
    }
}
