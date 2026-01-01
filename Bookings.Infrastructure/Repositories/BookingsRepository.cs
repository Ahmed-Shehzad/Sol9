using Bookings.Application.Contexts;
using Bookings.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Repositories;

public sealed class BookingsRepository : IBookingsRepository
{
    private readonly IBookingsDbContext _context;

    public BookingsRepository(IBookingsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Bookings
            .AsNoTracking()
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Booking?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.OrderId == orderId, cancellationToken)
            .ConfigureAwait(false);

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
        => await _context.Bookings.AddAsync(booking, cancellationToken).ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
