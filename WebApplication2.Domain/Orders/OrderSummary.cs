namespace WebApplication2.Domain.Orders;

public sealed record OrderSummary(
    Guid OrderId,
    string CustomerName,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset ReceivedAt);
