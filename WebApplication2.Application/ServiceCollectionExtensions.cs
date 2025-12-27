using Intercessor;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2.Application.Orders;

namespace WebApplication2.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApplication2Application(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddIntercessor(builder =>
        {
            builder.RegisterFromAssembly(typeof(ApplyOrderCreatedCommandHandler).Assembly);
        });
        services.AddScoped<OrderIntegrationSaga>();
        return services;
    }
}
