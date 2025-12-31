using Bookings.Application.Contracts;
using Bookings.Application.Dtos;
using Bookings.Domain.Entities;

using Intercessor.Abstractions;

namespace Bookings.Application.Commands.CreateBooking;

public sealed record CreateBookingCommand(Guid OrderId, string CustomerName) : ICommand<BookingDto>;

public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingsRepository _repository;

    public CreateBookingCommandHandler(IBookingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<BookingDto> HandleAsync(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        Booking? existing = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return new BookingDto(
                existing.Id,
                existing.OrderId,
                existing.CustomerName,
                existing.Status,
                existing.CreatedAtUtc);

        var booking = Booking.Create(request.OrderId, request.CustomerName);
        await _repository.AddAsync(booking, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new BookingDto(
            booking.Id,
            booking.OrderId,
            booking.CustomerName,
            booking.Status,
            booking.CreatedAtUtc);
    }
}
