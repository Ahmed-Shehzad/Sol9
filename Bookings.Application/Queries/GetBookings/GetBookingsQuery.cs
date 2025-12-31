using Bookings.Application.Contracts;
using Bookings.Application.Dtos;

using Intercessor.Abstractions;

namespace Bookings.Application.Queries.GetBookings;

public sealed record GetBookingsQuery : IQuery<IReadOnlyList<BookingDto>>;

public sealed class GetBookingsQueryHandler : IQueryHandler<GetBookingsQuery, IReadOnlyList<BookingDto>>
{
    private readonly IBookingsRepository _repository;

    public GetBookingsQueryHandler(IBookingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<BookingDto>> HandleAsync(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Bookings.Domain.Entities.Booking> bookings = await _repository.GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

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
