using System.Text.Json.Serialization;

namespace Locations.Service.Osrm.Models;

public record Route(
    [property: JsonPropertyName("geometry")]
    Geometry Geometry,
    [property: JsonPropertyName("legs")] IReadOnlyList<Leg> Legs,
    [property: JsonPropertyName("weight_name")]
    string WeightName,
    [property: JsonPropertyName("weight")] double Weight,
    [property: JsonPropertyName("duration")]
    int Duration,
    [property: JsonPropertyName("distance")]
    int Distance
);