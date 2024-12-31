using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record AddressDto(
    [property: JsonPropertyName("geography")]
    GeographyDto Geography,
    [property: JsonPropertyName("street")] string Street,
    [property: JsonPropertyName("number")] string Number,
    [property: JsonPropertyName("zipCode")]
    string ZipCode,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("country")]
    string Country
);