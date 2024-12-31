using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record OrderItemDto(
    [property: JsonPropertyName("orderId")]
    Ulid? OrderId,
    [property: JsonPropertyName("productId")]
    Ulid? ProductId,
    [property: JsonPropertyName("stopItemId")]
    Ulid? StopItemId,
    [property: JsonPropertyName("tripId")] Ulid? TripId,
    [property: JsonPropertyName("orderItemInfo")]
    OrderItemInfoDto OrderItemInfo,
    [property: JsonPropertyName("id")] Ulid? Id
);