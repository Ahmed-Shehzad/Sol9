using System.Text.Json.Serialization;

using Bookings.Domain.Entities;

namespace Bookings.Application.Dtos;

public sealed record BookingDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("orderId")] Guid OrderId,
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("status")] BookingStatus Status,
    [property: JsonPropertyName("createdAtUtc")] DateTime CreatedAtUtc);
