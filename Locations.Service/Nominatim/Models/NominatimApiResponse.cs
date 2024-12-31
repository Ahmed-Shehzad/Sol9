using System.Text.Json.Serialization;

namespace Locations.Service.Nominatim.Models;

public record NominatimApiResponse(
    [property: JsonPropertyName("place_id")]
    int PlaceId,
    [property: JsonPropertyName("licence")]
    string Licence,
    [property: JsonPropertyName("osm_type")]
    string OsmType,
    [property: JsonPropertyName("osm_id")] int OsmId,
    [property: JsonPropertyName("lat")] string Lat,
    [property: JsonPropertyName("lon")] string Lon,
    [property: JsonPropertyName("class")] string Class,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("place_rank")]
    int PlaceRank,
    [property: JsonPropertyName("importance")]
    double Importance,
    [property: JsonPropertyName("addresstype")]
    string Addresstype,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("display_name")]
    string DisplayName,
    [property: JsonPropertyName("address")]
    Address Address,
    [property: JsonPropertyName("boundingbox")]
    IReadOnlyList<string> Boundingbox
);