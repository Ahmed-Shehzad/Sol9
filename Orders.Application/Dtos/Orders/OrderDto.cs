using System;

namespace Orders.Application.Dtos.Orders;

public sealed record OrderDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAtUtc);
