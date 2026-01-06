using System.Text.Json.Serialization;

namespace Bookings.API.Responses;

public sealed record ResourceDto<T>(
    [property: JsonPropertyName("data")] T Data,
    [property: JsonPropertyName("_links")] IReadOnlyDictionary<string, LinkDto> Links);
