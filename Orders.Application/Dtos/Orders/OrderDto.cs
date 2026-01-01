using System;
using System.Text.Json.Serialization;

using Orders.Domain.Entities;

namespace Orders.Application.Dtos.Orders;

public sealed record OrderDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("status")] OrderStatus Status,
    [property: JsonPropertyName("createdAtUtc")] DateTime CreatedAtUtc);
