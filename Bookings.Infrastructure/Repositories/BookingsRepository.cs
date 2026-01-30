using System.Linq.Expressions;

using Bookings.Application.Contexts;
using Bookings.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Sol9.Core;

namespace Bookings.Infrastructure.Repositories;

public sealed class BookingsRepository : Repository<Booking>, IBookingsRepository
{
    private readonly IBookingsDbContext _context;

    public BookingsRepository(IBookingsDbContext context) : base(context)
    {
        _context = context;
    }

    public async override Task<IReadOnlyList<Booking>> GetManyAsync(
        Expression<Func<Booking, bool>> expression,
        Expression<Func<Booking, object>>? orderByExpression = null,
        bool orderByDescending = false,
        CancellationToken cancellationToken = default)
    {
        // Build the query with the filter expression
        IQueryable<Booking> query = _context.Bookings.Where(expression);

        // Apply ordering if provided
        if (orderByExpression != null)
            query = orderByDescending ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression);

        // Execute and return the result asynchronously
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async override Task<Booking?> GetAsync(Expression<Func<Booking, bool>> expression,
        CancellationToken cancellationToken = default) =>
        await _context.Bookings.FirstOrDefaultAsync(
                expression,
                cancellationToken)
            .ConfigureAwait(false);

    public async override Task AddAsync(Booking booking,
        CancellationToken cancellationToken = default) =>
        await _context.Bookings.AddAsync(
                booking,
                cancellationToken)
            .ConfigureAwait(false);
    public async override Task AddAsync(IEnumerable<Booking> entities,
        CancellationToken cancellationToken = default) =>
        await _context.Bookings.AddRangeAsync(
                entities,
                cancellationToken)
            .ConfigureAwait(false);
    public override void Update(Booking entity) => _context.Bookings.Update(entity);
    public override void Update(IEnumerable<Booking> entities) => _context.Bookings.UpdateRange(entities);
    public override void Delete(Booking entity) => _context.Bookings.Remove(entity);
    public override void Delete(IEnumerable<Booking> entities) => _context.Bookings.RemoveRange(entities);
}
