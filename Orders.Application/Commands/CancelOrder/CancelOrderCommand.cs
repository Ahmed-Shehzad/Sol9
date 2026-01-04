using Intercessor.Abstractions;

using Orders.Application.Contexts;
using Orders.Domain.Entities;

using Verifier;

namespace Orders.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<Guid>;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        _ = RuleFor(command => command.OrderId)
            .NotNull()
            .NotEmpty("Order Id must not be empty.");
    }
}

public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, Guid>
{
    private readonly IOrdersRepository _ordersRepository;

    public CancelOrderCommandHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }
    public async Task<Guid> HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken = default)
    {
        Order? order = await _ordersRepository.GetAsync(o => o.Id == request.OrderId, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(order);

        order.MarkCancelled();
        await _ordersRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return order.Id;
    }
}
