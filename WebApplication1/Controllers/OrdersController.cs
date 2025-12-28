using Intercessor.Abstractions;

using Microsoft.AspNetCore.Mvc;

using WebApplication1.Application.Orders;

namespace WebApplication1.Controllers;

/// <summary>
/// Provides endpoints for creating orders.
/// </summary>
[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="sender">The sender to use for sending commands.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null.</exception>
    public OrdersController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    /// <summary>
    /// Creates a new order asynchronously.
    /// </summary>
    /// <param name="request">The create order request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="CreateOrderResult"/> containing the ID of the created order.</returns>
    /// <response code="202">Returns the created order result.</response>
    /// <response code="400">If the request is null.</response>
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
}

public sealed record CreateOrderRequest(string CustomerName, decimal Total);
