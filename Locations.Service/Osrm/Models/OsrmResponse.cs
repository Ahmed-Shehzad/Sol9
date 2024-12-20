using System.Text.Json.Serialization;

namespace Locations.Service.Osrm.Models;

public record OsrmResponse(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("routes")] IReadOnlyList<Route> Routes,
    [property: JsonPropertyName("waypoints")]
    IReadOnlyList<Waypoint> Waypoints
);