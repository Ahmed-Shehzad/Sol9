using WebApplication1.Application.Orders;
using WebApplication1.Domain.Orders;

namespace WebApplication1.Infrastructure.Orders;

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly object _sync = new();
    private readonly List<Order> _orders = [];

    public Task SaveAsync(Order order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync) _orders.Add(order);
        return Task.CompletedTask;
    }
}
