using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record DocumentInfoDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("url")] string Url
);