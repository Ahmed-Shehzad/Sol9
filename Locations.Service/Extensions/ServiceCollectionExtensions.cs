using BuildingBlocks.Utilities.Policies;
using Locations.Service.Contracts;
using Locations.Service.Nominatim;
using Locations.Service.Nominatim.Settings;
using Locations.Service.Osrm;
using Locations.Service.Osrm.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Locations.Service.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Nominatim API client and service to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddNominatimApi(this IServiceCollection services)
    {
        // Register Refit Client with settings-driven configuration
        services.AddRefitClient<INominatimApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var apiSettings = serviceProvider.GetRequiredService<IOptions<NominatimApiSettings>>().Value;
                client.BaseAddress = new Uri(apiSettings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutInSeconds);
            }).AddPolicyHandler(RetryPolicyExtensions.GetRetryPolicy());

        services.AddScoped<INominatimService, NominatimService>();
        return services;
    }

    /// <summary>
    /// Adds the Open Street Map API client and service to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOpenStreetMapApi(this IServiceCollection services)
    {
        // Register Refit Client with settings-driven configuration
        services.AddRefitClient<IOsrmApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var apiSettings = serviceProvider.GetRequiredService<IOptions<OsrmApiSettings>>().Value;
                client.BaseAddress = new Uri(apiSettings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutInSeconds);
            }).AddPolicyHandler(RetryPolicyExtensions.GetRetryPolicy());

        services.AddScoped<IOpenStreetMapService, OpenStreetMapService>();
        return services;
    }
}