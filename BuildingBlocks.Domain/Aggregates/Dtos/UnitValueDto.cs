using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Aggregates.Dtos;

public record UnitValueDto<T>(
    [property: JsonPropertyName("value")] T Value,
    [property: JsonPropertyName("unit")] string? Unit
);