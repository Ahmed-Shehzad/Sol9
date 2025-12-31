using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Orders.Application;
using Orders.Application.Contracts;
using Orders.Infrastructure.Contexts;

namespace Orders.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddApplication();

        string? connectionString = configuration.GetConnectionString("Orders");
        if (string.IsNullOrWhiteSpace(connectionString)) return services;

        _ = services.AddDbContext<OrdersDbContext>(options =>
        {
            _ = options.UseNpgsql(connectionString);
        });

        _ = services.AddScoped<IOrdersDbContext>(sp => sp.GetRequiredService<OrdersDbContext>());
        _ = services.AddScoped<IReadOnlyOrdersDbContext>(sp => sp.GetRequiredService<OrdersDbContext>());
        return services;
    }
}
