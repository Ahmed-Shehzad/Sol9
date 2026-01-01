using Verifier;

namespace Bookings.Application.Commands.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        _ = RuleFor(command => command.OrderId)
            .Must(id => id != Guid.Empty, "OrderId must not be empty.");

        _ = RuleFor(command => command.CustomerName)
            .NotEmpty("CustomerName must not be empty.");
    }
}
