using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Application.Integration;
using WebApplication1.Application.Orders;
using WebApplication1.Infrastructure.Integration;
using WebApplication1.Infrastructure.Orders;

namespace WebApplication1.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApplication1Infrastructure(
        this IServiceCollection services,
        Action<IntegrationEventPublisherOptions> configurePublisher)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurePublisher);

        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IIntegrationEventPublisher, IntegrationEventPublisher>();
        services.Configure(configurePublisher);

        return services;
    }
}
