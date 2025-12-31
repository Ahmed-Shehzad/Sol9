using Intercessor;

using Microsoft.Extensions.DependencyInjection;

namespace Orders.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddIntercessor(options =>
        {
            options.RegisterFromAssembly(typeof(OrdersApplication).Assembly);
        });
        return services;
    }
}
