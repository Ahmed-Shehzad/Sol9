using Bookings.Application.Contexts;
using Bookings.Application.Dtos;

using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

using Verifier;

namespace Bookings.Application.Queries.GetBookingByOrderId;

public sealed record GetBookingByOrderIdQuery(Ulid OrderId) : IQuery<BookingDto?>;

public sealed class GetBookingByOrderIdQueryValidator : AbstractValidator<GetBookingByOrderIdQuery>
{
    public GetBookingByOrderIdQueryValidator()
    {
        _ = RuleFor(query => query.OrderId)
            .Must(id => id != Ulid.Empty, "OrderId must not be empty.");
    }
}


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
