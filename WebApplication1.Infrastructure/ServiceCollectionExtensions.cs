using Intercessor.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        _ = services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.TryAddEnumerable(ServiceDescriptor.Transient(
            typeof(INotificationHandler<>),
            typeof(IntegrationEventPublisher<>)));
        _ = services.Configure(configurePublisher);

        return services;
    }
}
