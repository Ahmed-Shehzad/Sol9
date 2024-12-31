using System.Text.Json.Serialization;

namespace Locations.Service.Nominatim.Models;

public record Address(
    [property: JsonPropertyName("house_number")]
    string HouseNumber,
    [property: JsonPropertyName("road")] string Road,
    [property: JsonPropertyName("neighbourhood")]
    string Neighbourhood,
    [property: JsonPropertyName("suburb")] string Suburb,
    [property: JsonPropertyName("borough")]
    string Borough,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("ISO3166-2-lvl4")]
    string Iso31662Lvl4,
    [property: JsonPropertyName("postcode")]
    string Postcode,
    [property: JsonPropertyName("country")]
    string Country,
    [property: JsonPropertyName("country_code")]
    string CountryCode
);