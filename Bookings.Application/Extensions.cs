using Intercessor;

using Microsoft.Extensions.DependencyInjection;

using Verifier;

namespace Bookings.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddIntercessor(options =>
        {
            options.RegisterFromAssembly(typeof(BookingsApplication).Assembly);
        });
        _ = services.AddVerifier(builder =>
        {
            builder.RegisterFromAssembly(typeof(BookingsApplication).Assembly);
        });
        return services;
    }
}
