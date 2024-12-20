using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using Locations.Service.Osrm.Enumerations;
using Locations.Service.Osrm.Models;

namespace Locations.Service.Osrm;

public interface IOpenStreetMapService
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
    Task<OsrmResponse> GetRouteAsync(List<Coordinates> coordinates, bool alternatives = false, bool steps = false, bool annotations = false,
        GeoFormat? geoFormat = null, GeoOverview? overview = null);
}