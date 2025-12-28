using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Sol9.Eventing;

using Transponder.Abstractions;
using Transponder.Contracts.Orders;

namespace WebApplication1.Controllers;

/// <summary>
/// Provides endpoints for order follow-ups.
/// </summary>
[ApiController]
[Route("orders/{orderId:guid}/followups")]
public sealed class OrderFollowUpsController : ControllerBase
{
    private readonly IBus _bus;
    private readonly IntegrationEventPublisherOptions _publisherOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderFollowUpsController"/> class.
    /// </summary>
    /// <param name="bus">The bus to use for scheduling messages.</param>
    /// <param name="publisherOptions">The publisher options.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null.</exception>
    public OrderFollowUpsController(
        IBus bus,
        IOptions<IntegrationEventPublisherOptions> publisherOptions)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _publisherOptions = publisherOptions?.Value ?? throw new ArgumentNullException(nameof(publisherOptions));
    }

    /// <summary>
    /// Schedules a follow-up for an order asynchronously.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="request">The schedule follow-up request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ScheduleOrderFollowUpResponse"/> containing the scheduled follow-up details.</returns>
    /// <response code="202">Returns the scheduled follow-up response.</response>
    /// <response code="400">If the request is null or delay is invalid.</response>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ScheduleOrderFollowUpResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScheduleOrderFollowUpResponse>> ScheduleAsync(
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
}

public sealed record ScheduleOrderFollowUpRequest(int DelaySeconds);

public sealed record ScheduleOrderFollowUpResponse(
    Guid TokenId,
    DateTimeOffset ScheduledFor,
    Uri DestinationAddress);
