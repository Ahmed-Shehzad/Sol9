using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orders.Application.Dtos;

public record OrderDocumentDto(
    [property: JsonPropertyName("documentInfo")]
    DocumentInfoDto DocumentInfo,
    [property: JsonPropertyName("orderId")]
    Ulid? OrderId,
    [property: JsonPropertyName("metadata")]
    JsonElement? MetaData,
    [property: JsonPropertyName("tenantId")]
    Ulid? TenantId,
    [property: JsonPropertyName("userId")] Ulid? UserId,
    [property: JsonPropertyName("id")] Ulid? Id
);