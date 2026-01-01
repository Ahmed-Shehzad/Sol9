using Bookings.Application.Contexts;
using Bookings.Application.Dtos;

using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Bookings.Application.Queries.GetBookingByOrderId;

public sealed record GetBookingByOrderIdQuery(Guid OrderId) : IQuery<BookingDto?>;

public sealed class GetBookingByOrderIdQueryHandler : IQueryHandler<GetBookingByOrderIdQuery, BookingDto?>
{
    private readonly IReadOnlyBookingsDbContext _context;

    public GetBookingByOrderIdQueryHandler(IReadOnlyBookingsDbContext context)
    {
        _context = context;
    }

    public async Task<BookingDto?> HandleAsync(GetBookingByOrderIdQuery request, CancellationToken cancellationToken = default)
    {
        Domain.Entities.Booking? booking =
            await _context.Bookings.FirstOrDefaultAsync(b => b.OrderId == request.OrderId, cancellationToken).ConfigureAwait(false);

        return booking is null
            ? null
            : new BookingDto(
                booking.Id,
                booking.OrderId,
                booking.CustomerName,
                booking.Status,
                booking.CreatedAtUtc);
    }
}
