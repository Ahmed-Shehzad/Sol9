using Microsoft.Extensions.DependencyInjection;
using WebApplication2.Application.Orders;
using WebApplication2.Infrastructure.Orders;

namespace WebApplication2.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApplication2Infrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IOrderReadRepository, InMemoryOrderReadRepository>();

        return services;
    }
}
