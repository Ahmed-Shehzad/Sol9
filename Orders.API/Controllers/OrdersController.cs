using Asp.Versioning;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Mvc;

using Orders.API.Requests;
using Orders.Application.Commands.CancelOrder;
using Orders.Application.Commands.CreateOrder;
using Orders.Application.Dtos.Orders;
using Orders.Application.Queries.GetOrderById;
using Orders.Application.Queries.GetOrders;

using Sol9.Core;
using Orders.API.Responses;

namespace Orders.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrdersController : ControllerBase
{
    private const string GetAction = "Get";
    private const string GetByIdAction = "GetById";
    private const string CancelAction = "CancelOrder";
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CollectionDto<ResourceDto<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionDto<ResourceDto<OrderDto>>>> GetAsync(CancellationToken cancellationToken = default)
    {
        const string key = nameof(GetOrdersQuery);
        IReadOnlyList<OrderDto> orders = await _sender.SendAsync(new GetOrdersQuery(key), cancellationToken).ConfigureAwait(false);
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        IReadOnlyList<ResourceDto<OrderDto>> items = [.. orders.Select(order => new ResourceDto<OrderDto>(order, BuildOrderLinks(order.Id, version)))];

        return Ok(new CollectionDto<ResourceDto<OrderDto>>(items, BuildCollectionLinks(version)));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto<OrderDto>>> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        OrderDto? order = await _sender.SendAsync(new GetOrderByIdQuery(id.ToUlid()), cancellationToken).ConfigureAwait(false);
        if (order is null) return NotFound();
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return Ok(new ResourceDto<OrderDto>(order, BuildOrderLinks(order.Id, version)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto<Guid>>> CreateAsync([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        Guid orderId = await _sender
            .SendAsync(new CreateOrderCommand(request.CustomerName, request.TotalAmount), cancellationToken)
            .ConfigureAwait(false);

        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        var resource = new ResourceDto<Guid>(orderId, BuildOrderLinks(orderId, version));
        return CreatedAtAction(GetByIdAction, new { id = orderId, version }, resource);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ResourceDto<Guid>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceDto<Guid>>> CancelOrderAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        await _sender
            .SendAsync(new CancelOrderCommand(id.ToUlid()), cancellationToken)
            .ConfigureAwait(false);

        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return Ok(new ResourceDto<Guid>(id, BuildOrderLinks(id, version)));
    }

    private IReadOnlyDictionary<string, LinkDto> BuildOrderLinks(Guid id, string? version)
        => new Dictionary<string, LinkDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["self"] = new LinkDto(Url.Action(GetByIdAction, new { id, version }) ?? string.Empty, "GET"),
            ["cancel"] = new LinkDto(Url.Action(CancelAction, new { id, version }) ?? string.Empty, "POST"),
            ["collection"] = new LinkDto(Url.Action(GetAction, new { version }) ?? string.Empty, "GET")
        };

    private IReadOnlyDictionary<string, LinkDto> BuildCollectionLinks(string? version)
        => new Dictionary<string, LinkDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["self"] = new LinkDto(Url.Action(GetAction, new { version }) ?? string.Empty, "GET"),
            ["create"] = new LinkDto(Url.Action(GetAction, new { version }) ?? string.Empty, "POST")
        };
}
