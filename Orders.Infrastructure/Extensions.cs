using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Orders.Application;
using Orders.Application.Contexts;
using Orders.Infrastructure.Contexts;
using Orders.Infrastructure.Interceptors;
using Orders.Infrastructure.Repositories;

namespace Orders.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddApplication();

        string? connectionString = configuration.GetConnectionString("Orders");
        if (string.IsNullOrWhiteSpace(connectionString)) return services;

        _ = services.AddDbContext<OrdersDbContext>((serviceProvider, options) =>
        {
            _ = options.UseNpgsql(connectionString);

            IPublisher publisher = serviceProvider.GetRequiredService<IPublisher>();
            Transponder.Abstractions.IBus bus = serviceProvider.GetRequiredService<Transponder.Abstractions.IBus>();
            Transponder.Transports.Abstractions.IMessageSerializer serializer = serviceProvider
                .GetRequiredService<Transponder.Transports.Abstractions.IMessageSerializer>();
            Transponder.Persistence.EntityFramework.PostgreSql.Abstractions.IPostgreSqlStorageOptions storageOptions =
                serviceProvider.GetRequiredService<
                    Transponder.Persistence.EntityFramework.PostgreSql.Abstractions.IPostgreSqlStorageOptions>();
            _ = options.AddInterceptors(
                new AuditableInterceptor(),
                new DomainEventDispatchInterceptor(publisher),
                new IntegrationEventDispatchInterceptor(publisher, bus, serializer, storageOptions));
        });

        _ = services.AddDbContext<ReadOnlyOrdersDbContext>(options =>
        {
            _ = options.UseNpgsql(connectionString);
            _ = options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        _ = services.AddScoped<IOrdersRepository, OrdersRepository>();
        _ = services.AddScoped<IOrdersDbContext>(sp => sp.GetRequiredService<OrdersDbContext>());
        _ = services.AddScoped<IReadOnlyOrdersDbContext>(sp => sp.GetRequiredService<ReadOnlyOrdersDbContext>());
        return services;
    }
}
