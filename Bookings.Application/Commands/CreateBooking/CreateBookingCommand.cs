using Bookings.Application.Contexts;
using Bookings.Domain.Entities;

using Intercessor.Abstractions;

using Verifier;

namespace Bookings.Application.Commands.CreateBooking;

public sealed record CreateBookingCommand(Ulid OrderId, string CustomerName) : ICommand<Guid>;

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

public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, Guid>
{
    private readonly IBookingsRepository _repository;

    public CreateBookingCommandHandler(IBookingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(CreateBookingCommand request, CancellationToken cancellationToken = default)
    {
        Booking? existing = await _repository.GetAsync(b => b.OrderId == request.OrderId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return existing.Id.ToGuid();

        var booking = Booking.Create(request.OrderId, request.CustomerName);
        await _repository.AddAsync(booking, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return booking.Id.ToGuid();
    }
}
