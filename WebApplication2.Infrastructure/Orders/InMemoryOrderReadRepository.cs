using WebApplication2.Application.Orders;
using WebApplication2.Domain.Orders;

namespace WebApplication2.Infrastructure.Orders;

internal sealed class InMemoryOrderReadRepository : IOrderReadRepository
{
    private readonly object _sync = new();
    private readonly List<OrderSummary> _orders = [];

    public Task AddAsync(OrderSummary order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync) _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OrderSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
            return Task.FromResult<IReadOnlyList<OrderSummary>>(_orders.ToArray());
    }
}
