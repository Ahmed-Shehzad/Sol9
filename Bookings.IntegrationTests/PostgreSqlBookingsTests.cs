using Bookings.Domain.Entities;
using Bookings.Infrastructure.Contexts;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

using Xunit;

namespace Bookings.IntegrationTests;

public sealed class PostgreSqlBookingsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
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
        await context.Database.MigrateAsync();

        var booking = Booking.Create(Guid.NewGuid(), "Test Customer");
        _ = context.Bookings.Add(booking);
        _ = await context.SaveChangesAsync();

        Booking? stored = await context.Bookings.SingleOrDefaultAsync(b => b.Id == booking.Id);
        Assert.NotNull(stored);
        Assert.Equal("Created", stored!.Status);
    }
}
