using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record OrderDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("description")]
    string Description,
    [property: JsonPropertyName("status")] OrderStatusDto Status,
    [property: JsonPropertyName("billingAddress")]
    AddressDto? BillingAddress,
    [property: JsonPropertyName("shippingAddress")]
    AddressDto? ShippingAddress,
    [property: JsonPropertyName("transportAddress")]
    AddressDto? TransportAddress,
    [property: JsonPropertyName("timeFrames")]
    IReadOnlyList<TimeFrameDto> TimeFrames,
    [property: JsonPropertyName("items")] IReadOnlyList<OrderItemDto> Items,
    [property: JsonPropertyName("documents")]
    IReadOnlyList<OrderDocumentDto> Documents,
    [property: JsonPropertyName("depots")] IReadOnlyList<DepotDto> Depots,
    [property: JsonPropertyName("tenantId")]
    Ulid? TenantId,
    [property: JsonPropertyName("userId")] Ulid? UserId,
    [property: JsonPropertyName("id")] Ulid? Id
);