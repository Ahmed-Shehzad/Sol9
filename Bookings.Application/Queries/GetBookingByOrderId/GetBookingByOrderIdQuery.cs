using Bookings.Application.Contracts;
using Bookings.Application.Dtos;

using Intercessor.Abstractions;

namespace Bookings.Application.Queries.GetBookingByOrderId;

public sealed record GetBookingByOrderIdQuery(Guid OrderId) : IQuery<BookingDto?>;

public sealed class GetBookingByOrderIdQueryHandler : IQueryHandler<GetBookingByOrderIdQuery, BookingDto?>
{
    private readonly IBookingsRepository _repository;

    public GetBookingByOrderIdQueryHandler(IBookingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<BookingDto?> HandleAsync(GetBookingByOrderIdQuery request, CancellationToken cancellationToken)
    {
        Bookings.Domain.Entities.Booking? booking = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
            .ConfigureAwait(false);

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
