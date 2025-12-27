using WebApplication1.Domain.Orders;

namespace WebApplication1.Application.Orders;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken);
}
