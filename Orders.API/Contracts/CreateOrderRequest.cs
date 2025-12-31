namespace Orders.API.Contracts;

public sealed record CreateOrderRequest(string CustomerName, decimal TotalAmount);
