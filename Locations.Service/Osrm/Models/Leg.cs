using System.Text.Json.Serialization;

namespace Locations.Service.Osrm.Models;

public record Leg(
    [property: JsonPropertyName("steps")] IReadOnlyList<object> Steps,
    [property: JsonPropertyName("summary")]
    string Summary,
    [property: JsonPropertyName("weight")] double Weight,
    [property: JsonPropertyName("duration")]
    double Duration,
    [property: JsonPropertyName("distance")]
    double Distance
);