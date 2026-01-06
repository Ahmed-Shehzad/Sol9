using System.Text.Json.Serialization;

namespace Bookings.API.Responses;

public sealed record LinkDto(
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("method")] string Method);
