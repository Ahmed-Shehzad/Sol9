using Intercessor.Abstractions;

using Orders.Application.Contexts;
using Orders.Domain.Entities;

using Verifier;

namespace Orders.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) : ICommand<Guid>;

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

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrdersRepository _ordersRepository;

    public CreateOrderCommandHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<Guid> HandleAsync(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = Order.Create(request.CustomerName, request.TotalAmount);
        _ = _ordersRepository.AddAsync(order, cancellationToken);
        await _ordersRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return order.Id;
    }
}
