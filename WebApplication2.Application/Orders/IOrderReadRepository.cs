using WebApplication2.Domain.Orders;

namespace WebApplication2.Application.Orders;

public interface IOrderReadRepository
{
    Task AddAsync(OrderSummary order, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderSummary>> GetAllAsync(CancellationToken cancellationToken);
}
