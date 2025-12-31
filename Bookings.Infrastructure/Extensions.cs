using Bookings.Application;
using Bookings.Application.Contracts;
using Bookings.Infrastructure.Contexts;
using Bookings.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddApplication();

        string? connectionString = configuration.GetConnectionString("Bookings");
        if (string.IsNullOrWhiteSpace(connectionString)) return services;

        _ = services.AddDbContext<BookingsDbContext>(options =>
        {
            _ = options.UseNpgsql(connectionString);
        });

        _ = services.AddScoped<IBookingsRepository, BookingsRepository>();
        return services;
    }
}
