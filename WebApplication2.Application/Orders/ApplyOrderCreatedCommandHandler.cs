using Intercessor.Abstractions;

using WebApplication2.Domain.Orders;

namespace WebApplication2.Application.Orders;

public sealed class ApplyOrderCreatedCommandHandler : ICommandHandler<ApplyOrderCreatedCommand>
{
    private readonly IOrderReadRepository _repository;

    public ApplyOrderCreatedCommandHandler(IOrderReadRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task HandleAsync(ApplyOrderCreatedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var summary = new OrderSummary(
            command.OrderId,
            command.CustomerName,
            command.Total,
            command.CreatedAt,
            DateTimeOffset.UtcNow);

        return _repository.AddAsync(summary, cancellationToken);
    }
}
