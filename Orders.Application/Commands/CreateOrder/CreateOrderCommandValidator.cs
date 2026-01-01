using Verifier;

namespace Orders.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        _ = RuleFor(command => command.CustomerName)
            .NotEmpty("CustomerName must not be empty.");

        _ = RuleFor(command => command.TotalAmount)
            .Must(amount => amount > 0, "TotalAmount must be greater than zero.");
    }
}
