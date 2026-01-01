using System.Text.Json.Serialization;

namespace Orders.API.Requests;

public sealed record CreateOrderRequest(
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount);
