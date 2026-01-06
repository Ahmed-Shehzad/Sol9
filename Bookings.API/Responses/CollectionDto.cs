using System.Text.Json.Serialization;

namespace Bookings.API.Responses;

public sealed record CollectionDto<T>(
    [property: JsonPropertyName("items")] IReadOnlyList<T> Items,
    [property: JsonPropertyName("_links")] IReadOnlyDictionary<string, LinkDto> Links);
