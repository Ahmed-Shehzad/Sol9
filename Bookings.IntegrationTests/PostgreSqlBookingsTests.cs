using System.Linq.Expressions;

using Bogus;

using Bookings.Application.Commands.CreateBooking;
using Bookings.Application.Contexts;
using Bookings.Application.Dtos;
using Bookings.Domain.Entities;
using Bookings.Infrastructure.Contexts;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

using Shouldly;

using Testcontainers.PostgreSql;

using Xunit;

namespace Bookings.IntegrationTests;

public sealed class PostgreSqlBookingsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17.6")
        .WithDatabase("bookings")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task BookingsDbContext_can_persist_bookingAsync()
    {
        DbContextOptions<BookingsDbContext> options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var context = new BookingsDbContext(options);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            await context.Database.MigrateAsync();

        var booking = Booking.Create(Ulid.NewUlid(), "Test Customer");
        _ = context.Bookings.Add(booking);
        _ = await context.SaveChangesAsync();

        Booking? stored = await context.Bookings.SingleOrDefaultAsync(b => b.Id == booking.Id);
        Assert.NotNull(stored);
        Assert.Equal(BookingStatus.Created, stored.Status);
    }

    [Fact]
    public async Task BookingsDbContext_can_persist_multiple_bookingsAsync()
    {
        List<Booking> bookings = new Faker<Booking>()
            .CustomInstantiator(f => Booking.Create(Ulid.NewUlid(), f.Name.FullName()))
            .Generate(3);

        DbContextOptions<BookingsDbContext> options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var context = new BookingsDbContext(options);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            await context.Database.MigrateAsync();

        await context.Bookings.AddRangeAsync(bookings);
        _ = await context.SaveChangesAsync();

        var ids = bookings.Select(b => b.Id).ToList();
        List<Booking> stored = await context.Bookings
            .Where(b => ids.Contains(b.Id))
            .ToListAsync();

        stored.Count.ShouldBe(bookings.Count);
        stored.Select(b => b.Id).ShouldBe(ids, ignoreOrder: true);
        stored.Select(b => b.CustomerName).ShouldBe(bookings.Select(b => b.CustomerName), ignoreOrder: true);
        stored.All(b => b.Status == BookingStatus.Created).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateBookingCommandHandler_returns_existing_booking_without_savingAsync()
    {
        var faker = new Faker();
        var orderId = Ulid.NewUlid();
        string existingCustomer = faker.Name.FullName();
        var existing = Booking.Create(orderId, existingCustomer);
        string ignoredCustomer = faker.Name.FullName();

        IBookingsRepository repository = Substitute.For<IBookingsRepository>();
        _ = repository.GetAsync(Arg.Is<Expression<Func<Booking, bool>>>(expression => expression.Compile()(existing)),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Booking?>(existing));

        var handler = new CreateBookingCommandHandler(repository);

        Guid bookingId = await handler.HandleAsync(new CreateBookingCommand(orderId, ignoredCustomer), CancellationToken.None);

        bookingId.ShouldBe(existing.Id.ToGuid());

        _ = await repository.Received(1).GetAsync(
            Arg.Is<Expression<Func<Booking, bool>>>(expression => expression.Compile()(existing)),
            Arg.Any<CancellationToken>());
        _ = repository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        _ = repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
