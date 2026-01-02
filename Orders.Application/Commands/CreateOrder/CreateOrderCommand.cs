using Intercessor.Abstractions;

using Orders.Application.Contexts;
using Orders.Application.Dtos.Orders;
using Orders.Domain.Entities;

using Sol9.Contracts.Bookings;

using Transponder.Abstractions;

using Verifier;

namespace Orders.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) : ICommand<OrderDto>;

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

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IClientFactory _clientFactory;

    public CreateOrderCommandHandler(IOrdersRepository ordersRepository, IClientFactory clientFactory)
    {
        _ordersRepository = ordersRepository;
        _clientFactory = clientFactory;
    }

    public async Task<OrderDto> HandleAsync(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = Order.Create(request.CustomerName, request.TotalAmount);
        _ = _ordersRepository.AddAsync(order, cancellationToken);
        await _ordersRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        IRequestClient<CreateBookingRequest> bookingClient = _clientFactory.CreateRequestClient<CreateBookingRequest>();
        CreateBookingResponse response = await bookingClient
            .GetResponseAsync<CreateBookingResponse>(new CreateBookingRequest(order.Id, order.CustomerName), cancellationToken)
            .ConfigureAwait(false);

        if ((BookingStatus)response.Status is not BookingStatus.Created)
            return new OrderDto(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.Status,
                order.CreatedAtUtc);

        order.MarkBooked();
        await _ordersRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OrderDto(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.Status,
            order.CreatedAtUtc);
    }
}
