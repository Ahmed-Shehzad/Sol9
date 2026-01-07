using System.Text.Json.Serialization;

namespace Sol9.Core.Hypermedia;

public sealed record LinkDto(
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("method")] string Method);
