using Asp.Versioning;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Mvc;

using Orders.API.Requests;
using Orders.Application.Commands.CancelOrder;
using Orders.Application.Commands.CreateOrder;
using Orders.Application.Dtos.Orders;
using Orders.Application.Queries.GetOrderById;
using Orders.Application.Queries.GetOrders;

namespace Orders.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderDto> orders = await _sender.SendAsync(new GetOrdersQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        OrderDto? order = await _sender.SendAsync(new GetOrderByIdQuery(id), cancellationToken).ConfigureAwait(false);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateAsync([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        Guid order = await _sender
            .SendAsync(new CreateOrderCommand(request.CustomerName, request.TotalAmount), cancellationToken)
            .ConfigureAwait(false);

        return Ok(order);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> CancelOrderAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        _ = await _sender
            .SendAsync(new CancelOrderCommand(id), cancellationToken)
            .ConfigureAwait(false);

        return Ok();
    }
}
