using System.Linq.Expressions;

using NSubstitute;

using Orders.Application.Contexts;
using Orders.Application.EventHandlers.Integration;
using Orders.Domain.Entities;
using Orders.Domain.Events.Integration;

using Shouldly;

using Sol9.Contracts.Bookings;

using Transponder.Abstractions;

using Xunit;

namespace Orders.IntegrationTests;

public sealed class OrderCreatedIntegrationEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_marks_order_booked_when_booking_created_Async()
    {
        var order = Order.Create("Test Customer", 42.5m);

        IOrdersRepository repository = Substitute.For<IOrdersRepository>();
        _ = repository.GetAsync(
                Arg.Is<Expression<Func<Order, bool>>>(expression => expression.Compile()(order)),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Order?>(order));

        IRequestClient<CreateBookingRequest> requestClient = Substitute.For<IRequestClient<CreateBookingRequest>>();
        _ = requestClient.GetResponseAsync<CreateBookingResponse>(
                Arg.Any<CreateBookingRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CreateBookingResponse(
                Ulid.NewUlid(),
                order.Id,
                (int)BookingStatus.Created)));

        IClientFactory clientFactory = Substitute.For<IClientFactory>();
        _ = clientFactory.CreateRequestClient<CreateBookingRequest>(Arg.Any<TimeSpan?>())
            .Returns(requestClient);

        var handler = new OrderCreatedIntegrationEventHandler(repository, clientFactory);

        await handler.HandleAsync(new OrderCreatedIntegrationEvent(order.Id), CancellationToken.None);

        order.Status.ShouldBe(OrderStatus.Booked);
        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _ = await repository.Received(1).GetAsync(
            Arg.Is<Expression<Func<Order, bool>>>(expression => expression.Compile()(order)),
            Arg.Any<CancellationToken>());
        _ = await requestClient.Received(1).GetResponseAsync<CreateBookingResponse>(
            Arg.Is<CreateBookingRequest>(request =>
                request.OrderId == order.Id && request.CustomerName == order.CustomerName),
            Arg.Any<CancellationToken>());
    }
}
