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
using Sol9.Core.Hypermedia;
using Sol9.Core.Pagination;

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
    public async Task<ActionResult<CollectionDto<ResourceDto<OrderDto>>>> GetAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        PaginationRequest paging = PaginationRequest.Normalize(page, pageSize);
        string key = $"{nameof(GetOrdersQuery)}:{paging.Page}:{paging.PageSize}";
        PagedResult<OrderDto> result = await _sender
            .SendAsync(new GetOrdersQuery(key, paging.Page, paging.PageSize), cancellationToken)
            .ConfigureAwait(false);
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        var meta = new PaginationMeta(result.Page, result.PageSize, result.TotalCount, result.TotalPages);
        return Ok(Hateoas.Collection(
            result.Items,
            order => BuildOrderLinks(order.Id, version),
            BuildCollectionLinks(version, result),
            meta));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto<OrderDto>>> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        OrderDto? order = await _sender.SendAsync(new GetOrderByIdQuery(id.ToUlid()), cancellationToken).ConfigureAwait(false);
        if (order is null) return NotFound();
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return Ok(Hateoas.Resource(order, BuildOrderLinks(order.Id, version)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto<Guid>>> CreateAsync([FromBody] CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        Guid orderId = await _sender
            .SendAsync(new CreateOrderCommand(request.CustomerName, request.TotalAmount), cancellationToken)
            .ConfigureAwait(false);

        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        ResourceDto<Guid> resource = Hateoas.Resource(orderId, BuildOrderLinks(orderId, version));
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
        return Ok(Hateoas.Resource(id, BuildOrderLinks(id, version)));
    }

    private IReadOnlyDictionary<string, LinkDto> BuildOrderLinks(Guid id, string? version)
        => Hateoas.Links(
            ("self", Url.Action(GetByIdAction, new { id, version }) ?? string.Empty, "GET"),
            ("cancel", Url.Action(CancelAction, new { id, version }) ?? string.Empty, "POST"),
            ("collection", Url.Action(GetAction, new { version, page = PaginationRequest.DefaultPage, pageSize = PaginationRequest.DefaultPageSize }) ?? string.Empty, "GET"));

    private IReadOnlyDictionary<string, LinkDto> BuildCollectionLinks(string? version, PagedResult<OrderDto> result)
    {
        var links = new Dictionary<string, LinkDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["self"] = Hateoas.Link(Url.Action(GetAction, new { version, page = result.Page, pageSize = result.PageSize }) ?? string.Empty, "GET"),
            ["create"] = Hateoas.Link(Url.Action(GetAction, new { version }) ?? string.Empty, "POST")
        };

        if (result.HasPrevious)
            links["prev"] = Hateoas.Link(
                Url.Action(GetAction, new { version, page = result.Page - 1, pageSize = result.PageSize }) ?? string.Empty,
                "GET");

        if (result.HasNext)
            links["next"] = Hateoas.Link(
                Url.Action(GetAction, new { version, page = result.Page + 1, pageSize = result.PageSize }) ?? string.Empty,
                "GET");

        return links;
    }
}
