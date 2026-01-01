using Bookings.Application.Contexts;
using Bookings.Application.Dtos;

using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Bookings.Application.Queries.GetBookings;

public sealed record GetBookingsQuery : IQuery<IReadOnlyList<BookingDto>>;

public sealed class GetBookingsQueryHandler : IQueryHandler<GetBookingsQuery, IReadOnlyList<BookingDto>>
{
    private readonly IReadOnlyBookingsDbContext _context;

    public GetBookingsQueryHandler(IReadOnlyBookingsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BookingDto>> HandleAsync(GetBookingsQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Domain.Entities.Booking> bookings = await _context.Bookings.ToListAsync(cancellationToken).ConfigureAwait(false);

        return bookings
            .Select(booking => new BookingDto(
                booking.Id,
                booking.OrderId,
                booking.CustomerName,
                booking.Status,
                booking.CreatedAtUtc))
            .ToList();
    }
}
