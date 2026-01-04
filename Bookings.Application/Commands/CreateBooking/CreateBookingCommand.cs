using Bookings.Application.Contexts;
using Bookings.Application.Dtos;
using Bookings.Domain.Entities;

using Intercessor.Abstractions;

using Verifier;

namespace Bookings.Application.Commands.CreateBooking;

public sealed record CreateBookingCommand(Ulid OrderId, string CustomerName) : ICommand<BookingDto>;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        _ = RuleFor(command => command.OrderId)
            .Must(id => id != Ulid.Empty, "OrderId must not be empty.");

        _ = RuleFor(command => command.CustomerName)
            .NotEmpty("CustomerName must not be empty.");
    }
}

public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingsRepository _repository;

    public CreateBookingCommandHandler(IBookingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<BookingDto> HandleAsync(CreateBookingCommand request, CancellationToken cancellationToken = default)
    {
        Booking? existing = await _repository.GetAsync(b => b.OrderId == request.OrderId, cancellationToken).ConfigureAwait(false);
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
