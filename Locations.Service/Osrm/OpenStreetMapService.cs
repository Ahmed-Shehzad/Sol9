using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using Locations.Service.Contracts;
using Locations.Service.Osrm.Enumerations;
using Locations.Service.Osrm.Models;

namespace Locations.Service.Osrm;

/// <summary>
/// Represents the OpenStreetMap service that interacts with the OSRM API.
/// </summary>
/// <param name="osrmApi">The OSRM API interface.</param>
public class OpenStreetMapService(IOsrmApi osrmApi) : IOpenStreetMapService
{
    /// <summary>
    /// Requests a route from OpenStreetMap (OSRM).
    /// </summary>
    /// <param name="coordinates">The list of coordinates for the route.</param>
    /// <param name="alternatives">Whether to provide alternative routes. Default is false.</param>
    /// <param name="steps">Whether to include step-by-step instructions. Default is false.</param>
    /// <param name="annotations">Whether to include additional metadata. Default is false.</param>
    /// <param name="geoFormat">The format of the returned geometry. Default is null.</param>
    /// <param name="overview">The level of detail for the route geometry. Default is null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an OsrmResponse object.</returns>
    public async Task<OsrmResponse> GetRouteAsync(List<Coordinates> coordinates, bool alternatives = false, bool steps = false,
        bool annotations = false,
        GeoFormat? geoFormat = null, GeoOverview? overview = null)
    {
        geoFormat ??= GeoFormat.Polyline;
        overview ??= GeoOverview.Full;

        var coordinatesString = string.Join(";", coordinates.Select(c => $"{c.Longitude},{c.Latitude}"));
        return await osrmApi.GetRouteAsync(coordinatesString, alternatives, steps, annotations, geoFormat.ToString(), overview.ToString());
    }
}