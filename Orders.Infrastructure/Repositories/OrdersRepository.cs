using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly IOrdersDbContext _context;

    public OrdersRepository(IOrdersDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken)
            .ConfigureAwait(false);

    public async Task AddAsync(Order order,
        CancellationToken cancellationToken = default)
        => await _context.Orders.AddAsync(order, cancellationToken).ConfigureAwait(false);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
