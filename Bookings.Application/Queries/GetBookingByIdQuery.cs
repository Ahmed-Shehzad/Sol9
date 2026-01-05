using Bookings.Application.Contexts;
using Bookings.Application.Dtos;
using Bookings.Domain.Entities;

using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

using Verifier;

namespace Bookings.Application.Queries;

public record GetBookingByIdQuery(Ulid Id) : IQuery<BookingDto?>;

public sealed class GetBookingByOrderIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
{
    public GetBookingByOrderIdQueryValidator()
    {
        _ = RuleFor(query => query.Id)
            .Must(id => id != Ulid.Empty, "Booking Id must not be empty.");
    }
}

public class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IReadOnlyBookingsDbContext _dbContext;
    public GetBookingByIdQueryHandler(IReadOnlyBookingsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookingDto?> HandleAsync(GetBookingByIdQuery request, CancellationToken cancellationToken = default)
    {
        Booking? booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

        return booking is null
            ? null
            : new BookingDto(
                booking.Id.ToGuid(),
                booking.OrderId.ToGuid(),
                booking.CustomerName,
                booking.Status,
                booking.CreatedAtUtc);
    }
}
