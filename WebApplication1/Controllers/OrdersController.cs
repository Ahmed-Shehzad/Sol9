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
    private readonly IScheduledMessageStore _scheduledMessageStore;

    public OrdersController(
        ISender sender,
        IBus bus,
        IOptions<IntegrationEventPublisherOptions> publisherOptions,
        IScheduledMessageStore scheduledMessageStore)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _publisherOptions = publisherOptions?.Value ?? throw new ArgumentNullException(nameof(publisherOptions));
        _scheduledMessageStore = scheduledMessageStore ?? throw new ArgumentNullException(nameof(scheduledMessageStore));
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

    [HttpDelete("followups/{tokenId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelScheduledFollowUpAsync(Guid tokenId, CancellationToken cancellationToken)
    {
        bool cancelled = await _scheduledMessageStore.CancelAsync(tokenId, cancellationToken).ConfigureAwait(false);
        return cancelled ? NoContent() : NotFound();
    }
}

public sealed record CreateOrderRequest(string CustomerName, decimal Total);

public sealed record ScheduleOrderFollowUpRequest(int DelaySeconds);

public sealed record ScheduleOrderFollowUpResponse(
    Guid TokenId,
    DateTimeOffset ScheduledFor,
    Uri DestinationAddress);
