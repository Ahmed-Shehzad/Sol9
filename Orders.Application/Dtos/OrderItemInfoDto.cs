using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Domain.Aggregates.Dtos;

namespace Orders.Application.Dtos;

public record OrderItemInfoDto(
    [property: JsonPropertyName("quantity")]
    UnitValueDto<decimal> Quantity,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("weight")] UnitValueDto<decimal> Weight,
    [property: JsonPropertyName("metadata")]
    JsonElement? MetaData
);