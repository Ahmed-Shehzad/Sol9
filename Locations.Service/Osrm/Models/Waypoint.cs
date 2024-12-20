using System.Text.Json.Serialization;

namespace Locations.Service.Osrm.Models;

public record Waypoint(
    [property: JsonPropertyName("hint")] string Hint,
    [property: JsonPropertyName("distance")]
    double Distance,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")]
    IReadOnlyList<double> Location
);