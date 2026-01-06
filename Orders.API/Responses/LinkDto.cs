using System.Text.Json.Serialization;

namespace Orders.API.Responses;

public sealed record LinkDto(
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("method")] string Method);
