using Intercessor.Abstractions;

namespace WebApplication1.Application.Orders;

public sealed record CreateOrderCommand(string CustomerName, decimal Total)
    : ICommand<CreateOrderResult>;
