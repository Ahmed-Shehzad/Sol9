using Locations.Service.Contracts;
using Locations.Service.Nominatim.Models;

namespace Locations.Service.Nominatim;

/// <summary>
/// Service for interacting with the Nominatim API.
/// </summary>
/// <param name="nominatimApi">The Nominatim API service instance.</param>
public class NominatimService(INominatimApi nominatimApi) : INominatimService
{
    /// <summary>
    /// Geocodes a query string to a list of Nominatim API responses.
    /// </summary>
    /// <param name="query">The query string to geocode.</param>
    /// <returns>A list of Nominatim API responses.</returns>
    public async Task<List<NominatimApiResponse>> GeocodeAsync(string query)
    {
        return await nominatimApi.GeocodeAsync(query);
    }

    /// <summary>
    /// Reverse geocodes latitude and longitude to a Nominatim API response.
    /// </summary>
    /// <param name="latitude">The latitude to reverse geocode.</param>
    /// <param name="longitude">The longitude to reverse geocode.</param>
    /// <returns>A Nominatim API response.</returns>
    public async Task<NominatimApiResponse> ReverseGeocodeAsync(double latitude, double longitude)
    {
        return await nominatimApi.ReverseGeocodeAsync(latitude, longitude);
    }
}