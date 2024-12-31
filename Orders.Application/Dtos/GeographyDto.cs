using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record GeographyDto(
    [property: JsonPropertyName("longitude")]
    double? Longitude,
    [property: JsonPropertyName("latitude")]
    double? Latitude
);