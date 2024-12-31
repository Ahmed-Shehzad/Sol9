using Locations.Service.Nominatim.Models;
using Refit;

namespace Locations.Service.Contracts;

public interface INominatimApi
{
    /// <summary>
    /// Searches for a location using the Nominatim API (Geocoding).
    /// </summary>
    /// <param name="q">The query string to search for.</param>
    /// <param name="format">The response format, default is "json".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of Nominatim API responses.</returns>
    [Get("/search")]
    Task<List<NominatimApiResponse>> GeocodeAsync([Query] string q, [Query] string format = "json");

    /// <summary>
    /// Reverse geocodes latitude and longitude using the Nominatim API.
    /// </summary>
    /// <param name="lat">The latitude to reverse geocode.</param>
    /// <param name="lon">The longitude to reverse geocode.</param>
    /// <param name="format">The response format, default is "json".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Nominatim API response.</returns>
    [Get("/reverse")]
    Task<NominatimApiResponse> ReverseGeocodeAsync([Query] double lat, [Query] double lon, [Query] string format = "json");
}