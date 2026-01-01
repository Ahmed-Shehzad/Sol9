using System.Threading;
using System.Threading.Tasks;

using Intercessor.Abstractions;

using Orders.Application.Contracts;
using Orders.Application.Dtos.Orders;
using Orders.Domain.Entities;

using Sol9.Contracts.Bookings;

using Transponder.Abstractions;

namespace Orders.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) : ICommand<OrderDto>;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrdersDbContext _dbContext;
    private readonly IClientFactory _clientFactory;

    public CreateOrderCommandHandler(IOrdersDbContext dbContext, IClientFactory clientFactory)
    {
        _dbContext = dbContext;
        _clientFactory = clientFactory;
    }

    public async Task<OrderDto> HandleAsync(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = Order.Create(request.CustomerName, request.TotalAmount);
        _ = _dbContext.Orders.Add(order);
        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
        _ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new OrderDto(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.Status,
            order.CreatedAtUtc);
    }
}
