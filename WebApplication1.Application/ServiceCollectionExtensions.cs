using Intercessor;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Application.Orders;

namespace WebApplication1.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApplication1Application(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddIntercessor(builder =>
        {
            builder.RegisterFromAssembly(typeof(CreateOrderHandler).Assembly);
        });
        return services;
    }
}
