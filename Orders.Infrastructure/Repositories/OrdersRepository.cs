using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Domain.Entities;

using Sol9.Core;

namespace Orders.Infrastructure.Repositories;

public class OrdersRepository : Repository<Order>, IOrdersRepository
{
    private readonly IOrdersDbContext _context;

    public OrdersRepository(IOrdersDbContext context) : base(context)
    {
        _context = context;
    }

    public async override Task<IReadOnlyList<Order>> GetListAsync(Expression<Func<Order, bool>> expression,
        Expression<Func<Order, object>>? orderByExpression = null,
        bool orderByDescending = false,
        CancellationToken cancellationToken = default)
    {
        // Build the query with the filter expression
        IQueryable<Order> query = _context.Orders.Where(expression);

        // Apply ordering if provided
        if (orderByExpression != null)
            query = orderByDescending ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression);

        // Execute and return the result asynchronously
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async override Task<Order?> GetAsync(Expression<Func<Order, bool>> expression,
        CancellationToken cancellationToken = default) =>
        await _context.Orders.FirstOrDefaultAsync(
                expression,
                cancellationToken)
            .ConfigureAwait(false);

    public async override Task AddAsync(Order order,
        CancellationToken cancellationToken = default) =>
        await _context.Orders.AddAsync(
                order,
                cancellationToken)
            .ConfigureAwait(false);

    public override void Update(Order entity) => _context.Orders.Update(entity);
    public override void Update(IEnumerable<Order> entities) => _context.Orders.UpdateRange(entities);
    public override void Delete(Order entity) => _context.Orders.Remove(entity);
    public override void Delete(IEnumerable<Order> entities) => _context.Orders.RemoveRange(entities);
}
