using System.Text.Json.Serialization;

namespace Bookings.API.Requests;

public sealed record CreateBookingRequest(
    [property: JsonPropertyName("orderId")] Guid OrderId,
    [property: JsonPropertyName("customerName")] string CustomerName);
