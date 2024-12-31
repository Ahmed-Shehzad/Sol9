using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record OrderStatusDto(
    [property: JsonPropertyName("key")] long? Key,
    [property: JsonPropertyName("value")] string Value
);