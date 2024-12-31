using System.Text.Json.Serialization;

namespace Locations.Service.Osrm.Models;

public record Geometry(
    [property: JsonPropertyName("coordinates")]
    IReadOnlyList<List<double>> Coordinates,
    [property: JsonPropertyName("type")] string Type
);