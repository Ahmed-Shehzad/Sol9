using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Orders.Domain.Entities;

namespace Orders.Application.Contexts;

public interface IOrdersRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
