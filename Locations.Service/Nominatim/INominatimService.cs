using Locations.Service.Nominatim.Models;

namespace Locations.Service.Nominatim;

/// <summary>
/// Interface for the Nominatim service.
/// </summary>
public interface INominatimService
{
    /// <summary>
    /// Geocodes a query string to a list of Nominatim API responses.
    /// </summary>
    /// <param name="query">The query string to geocode.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of Nominatim API responses.</returns>
    Task<List<NominatimApiResponse>> GeocodeAsync(string query);

    /// <summary>
    /// Reverse geocodes latitude and longitude to a Nominatim API response.
    /// </summary>
    /// <param name="latitude">The latitude to reverse geocode.</param>
    /// <param name="longitude">The longitude to reverse geocode.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Nominatim API response.</returns>
    Task<NominatimApiResponse> ReverseGeocodeAsync(double latitude, double longitude);
}