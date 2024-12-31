using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record DepotDto(
    [property: JsonPropertyName("depotId")]
    Ulid? DepotId);