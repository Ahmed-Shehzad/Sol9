using Intercessor.Abstractions;

namespace WebApplication2.Application.Orders;

public sealed record ApplyOrderCreatedCommand(
    Guid OrderId,
    string CustomerName,
    decimal Total,
    DateTimeOffset CreatedAt) : ICommand;
