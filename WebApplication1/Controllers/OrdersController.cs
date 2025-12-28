using Intercessor.Abstractions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Transponder.Abstractions;
using Transponder.Contracts.Orders;
using Transponder.Persistence.Abstractions;

using WebApplication1.Application.Orders;
using WebApplication1.Infrastructure.Integration;

namespace WebApplication1.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IBus _bus;
    private readonly IntegrationEventPublisherOptions _publisherOptions;

    public OrdersController(
        ISender sender,
        IBus bus,
        IOptions<IntegrationEventPublisherOptions> publisherOptions)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _publisherOptions = publisherOptions?.Value ?? throw new ArgumentNullException(nameof(publisherOptions));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateOrderResult>> CreateAsync(
        [FromBody] CreateOrderRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest();

        var command = new CreateOrderCommand(request.CustomerName, request.Total);
        CreateOrderResult result = await _sender.SendAsync(command, cancellationToken).ConfigureAwait(false);

        return Accepted($"/orders/{result.OrderId}", result);
    }

    [HttpPost("{orderId:guid}/followups/schedule")]
    [ProducesResponseType(typeof(ScheduleOrderFollowUpResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScheduleOrderFollowUpResponse>> ScheduleFollowUpAsync(
        Guid orderId,
        [FromBody] ScheduleOrderFollowUpRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.DelaySeconds <= 0) return BadRequest();

        Uri destination = _publisherOptions.DestinationAddress
                           ?? throw new InvalidOperationException("Integration event destination address is not configured.");

        DateTimeOffset scheduledFor = DateTimeOffset.UtcNow.AddSeconds(request.DelaySeconds);
        var integrationEvent = new OrderFollowUpScheduledIntegrationEvent(orderId, scheduledFor);

        IScheduledMessageHandle handle = await _bus.ScheduleSendAsync(
                destination,
                integrationEvent,
                scheduledFor,
                cancellationToken)
            .ConfigureAwait(false);

        var response = new ScheduleOrderFollowUpResponse(handle.TokenId, scheduledFor, destination);
        return Accepted(response);
    }

    /// <summary>
    /// Schedules a follow-up for an order to be sent at a specified time in the future.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order for which the follow-up is being scheduled.</param>
    /// <param name="request">The request containing the delay in seconds before the follow-up should be sent.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/>
    /// with a <see cref="ScheduleOrderFollowUpResponse"/> containing the scheduled message handle token ID,
    /// scheduled time, and destination address.
    /// </returns>
    /// <remarks>
    /// Returns HTTP 202 Accepted if the follow-up is successfully scheduled.
    /// Returns HTTP 400 Bad Request if the request is null or the delay is less than or equal to zero.
    /// Throws <see cref="InvalidOperationException"/> if the integration event destination address is not configured.
    /// </remarks>
}
}

public sealed record CreateOrderRequest(string CustomerName, decimal Total);

public sealed record ScheduleOrderFollowUpRequest(int DelaySeconds);

public sealed record ScheduleOrderFollowUpResponse(
    Guid TokenId,
    DateTimeOffset ScheduledFor,
    Uri DestinationAddress);
