using Locations.Service.Osrm.Models;
using Refit;

namespace Locations.Service.Contracts;

public interface IOsrmApi
{
    /// <summary>
    /// Requests a route from OpenStreetMap (OSRM).
    /// </summary>
    /// <param name="coordinates">The coordinates for the route, formatted as "longitude,latitude;longitude,latitude".</param>
    /// <param name="alternatives">Whether to provide alternative routes. Default is false.</param>
    /// <param name="steps">Whether to include step-by-step instructions. Default is false.</param>
    /// <param name="annotations">Whether to include additional metadata. Default is false.</param>
    /// <param name="geometries">The format of the returned geometry. Default is "polyline".</param>
    /// <param name="overview">The level of detail for the route geometry. Default is "simplified".</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an OsrmResponse object.</returns>
    [Get("/route/v1/driving/{coordinates}")]
    Task<OsrmResponse> GetRouteAsync(
        string coordinates,
        [Query] bool alternatives = false,
        [Query] bool steps = false,
        [Query] bool annotations = false,
        [Query] string geometries = "polyline",
        [Query] string overview = "simplified");
}