using Intercessor;

using Microsoft.Extensions.DependencyInjection;

using Verifier;

namespace Orders.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddIntercessor(options =>
        {
            options.RegisterFromAssembly(typeof(OrdersApplication).Assembly);
        });
        _ = services.AddVerifier(builder =>
        {
            builder.RegisterFromAssembly(typeof(OrdersApplication).Assembly);
        });
        return services;
    }
}
