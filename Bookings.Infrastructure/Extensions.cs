using Bookings.Application;
using Bookings.Application.Contexts;
using Bookings.Infrastructure.Contexts;
using Bookings.Infrastructure.Interceptors;
using Bookings.Infrastructure.Repositories;

using Intercessor.Abstractions;

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

        _ = services.AddDbContext<BookingsDbContext>((serviceProvider, options) =>
        {
            _ = options.UseNpgsql(connectionString);
            IPublisher publisher = serviceProvider.GetRequiredService<IPublisher>();
            _ = options.AddInterceptors(
                new AuditableInterceptor(),
                new DomainEventDispatchInterceptor(publisher),
                new IntegrationEventDispatcherInterceptor(publisher));
        });
        _ = services.AddDbContext<ReadOnlyBookingsDbContext>(options =>
        {
            _ = options.UseNpgsql(connectionString);
            _ = options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        _ = services.AddScoped<IBookingsRepository, BookingsRepository>();
        _ = services.AddScoped<IBookingsDbContext>(sp => sp.GetRequiredService<BookingsDbContext>());
        _ = services.AddScoped<IReadOnlyBookingsDbContext>(sp => sp.GetRequiredService<ReadOnlyBookingsDbContext>());
        return services;
    }
}
