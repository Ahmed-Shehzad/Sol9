using Intercessor;

using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddIntercessor(options =>
        {
            options.RegisterFromAssembly(typeof(BookingsApplication).Assembly);
        });
        return services;
    }
}
