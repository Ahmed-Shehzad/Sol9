using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookings.Infrastructure.Contexts;

public sealed class BookingsDbContextFactory : IDesignTimeDbContextFactory<BookingsDbContext>
{
    public BookingsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BookingsDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Bookings")
            ?? Environment.GetEnvironmentVariable("BOOKINGS_DB_CONNECTION")
            ?? "Host=localhost;Database=bookings;Username=postgres;Password=postgres";

        _ = optionsBuilder.UseNpgsql(connectionString);
        return new BookingsDbContext(optionsBuilder.Options);
    }
}
