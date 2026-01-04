using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Domain.Events.Integration;

using Sol9.Contracts.Bookings;
using Sol9.Core.Abstractions;

using Transponder.Abstractions;

namespace Orders.Application.EventHandlers.Integration;

public class OrderCancelledIntegrationEventHandler : IIntegrationEventHandler<OrderCancelledIntegrationEvent>
{
    private readonly IReadOnlyOrdersDbContext _context;
    private readonly IClientFactory _clientFactory;

    public OrderCancelledIntegrationEventHandler(IReadOnlyOrdersDbContext context, IClientFactory clientFactory) : base()
    {
        _context = context;
        _clientFactory = clientFactory;
    }
    public async Task HandleAsync(OrderCancelledIntegrationEvent notification, CancellationToken cancellationToken = default)
    {
        bool isOrderExists = await _context.Orders.AnyAsync(o => o.Id == notification.Id, cancellationToken).ConfigureAwait(false);

        if (!isOrderExists) return;

        IRequestClient<CancelBookingRequest> bookingClient = _clientFactory.CreateRequestClient<CancelBookingRequest>();
        _ = await bookingClient
            .GetResponseAsync<CancelBookingResponse>(new CancelBookingRequest(notification.Id, notification.CustomerName), cancellationToken)
            .ConfigureAwait(false);
    }
}
