using Orders.Application.Contexts;
using Orders.Domain.Entities;
using Orders.Domain.Events.Integration;

using Sol9.Contracts.Bookings;
using Sol9.Core.Abstractions;

using Transponder.Abstractions;

namespace Orders.Application.EventHandlers.Integration;

public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IClientFactory _clientFactory;

    public OrderCreatedIntegrationEventHandler(IOrdersRepository ordersRepository, IClientFactory clientFactory)
    {
        _ordersRepository = ordersRepository;
        _clientFactory = clientFactory;
    }

    public async Task HandleAsync(OrderCreatedIntegrationEvent notification, CancellationToken cancellationToken = default)
    {
        Order? order = await _ordersRepository.GetAsync(o => o.Id == notification.Id, cancellationToken).ConfigureAwait(false);

        if (order is null)
            ArgumentNullException.ThrowIfNull(order, nameof(order));

        IRequestClient<CreateBookingRequest> bookingClient = _clientFactory.CreateRequestClient<CreateBookingRequest>();
        CreateBookingResponse response = await bookingClient
            .GetResponseAsync<CreateBookingResponse>(new CreateBookingRequest(order.Id, order.CustomerName), cancellationToken)
            .ConfigureAwait(false);

        if ((BookingStatus)response.Status is BookingStatus.Created)
        {
            order.MarkBooked();
            await _ordersRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
