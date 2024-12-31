using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Aggregates.Dtos;

public record RangeDto<T>(
    [property: JsonPropertyName("from")] T From,
    [property: JsonPropertyName("to")] T To
);