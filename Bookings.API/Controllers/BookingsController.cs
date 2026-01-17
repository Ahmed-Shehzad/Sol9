using Asp.Versioning;

using Bookings.API.Requests;
using Bookings.Application.Commands.CreateBooking;
using Bookings.Application.Dtos;
using Bookings.Application.Queries;
using Bookings.Application.Queries.GetBookingByOrderId;
using Bookings.Application.Queries.GetBookings;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Mvc;

using Sol9.Core;
using Sol9.Core.Hypermedia;
using Sol9.Core.Pagination;

namespace Bookings.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/bookings")]
public class BookingsController : ControllerBase
{
    private const string GetAction = "Get";
    private const string GetByIdAction = "GetById";
    private const string GetByOrderAction = "GetByOrderId";
    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CollectionDto<ResourceDto<BookingDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionDto<ResourceDto<BookingDto>>>> GetAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var paging = PaginationRequest.Normalize(page, pageSize);
        string key = $"{nameof(GetBookingsQuery)}:{paging.Page}:{paging.PageSize}";
        PagedResult<BookingDto> result = await _sender
            .SendAsync(new GetBookingsQuery(key, paging.Page, paging.PageSize), cancellationToken)
            .ConfigureAwait(false);
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        var meta = new PaginationMeta(result.Page, result.PageSize, result.TotalCount, result.TotalPages);
        return Ok(Hateoas.Collection(
            result.Items,
            booking => BuildBookingLinks(booking.Id, booking.OrderId, version),
            BuildCollectionLinks(version, result),
            meta));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto<BookingDto>>> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        BookingDto? order = await _sender.SendAsync(new GetBookingByIdQuery(id.ToUlid()), cancellationToken).ConfigureAwait(false);
        if (order is null) return NotFound();
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return Ok(Hateoas.Resource(order, BuildBookingLinks(order.Id, order.OrderId, version)));
    }

    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(ResourceDto<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto<BookingDto>>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        BookingDto? booking = await _sender.SendAsync(new GetBookingByOrderIdQuery(orderId.ToUlid()), cancellationToken).ConfigureAwait(false);
        if (booking is null) return NotFound();
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return Ok(Hateoas.Resource(booking, BuildBookingLinks(booking.Id, booking.OrderId, version)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto<Guid>>> CreateAsync([FromBody] CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        Guid bookingId = await _sender.SendAsync(new CreateBookingCommand(request.OrderId.ToUlid(), request.CustomerName), cancellationToken).ConfigureAwait(false);
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        ResourceDto<Guid> resource = Hateoas.Resource(bookingId, BuildBookingLinks(bookingId, request.OrderId, version));
        return CreatedAtAction(GetByIdAction, new { id = bookingId, version }, resource);
    }

    private IReadOnlyDictionary<string, LinkDto> BuildBookingLinks(Guid id, Guid orderId, string? version)
        => Hateoas.Links(
            ("self", Url.Action(GetByIdAction, new { id, version }) ?? string.Empty, HttpMethod.Get.Method),
            ("byOrder", Url.Action(GetByOrderAction, new { orderId, version }) ?? string.Empty, HttpMethod.Get.Method),
            ("collection", Url.Action(GetAction, new { version, page = PaginationRequest.DefaultPage, pageSize = PaginationRequest.DefaultPageSize }) ?? string.Empty, HttpMethod.Get.Method));

    private IReadOnlyDictionary<string, LinkDto> BuildCollectionLinks(string? version, PagedResult<BookingDto> result)
    {
        var links = new Dictionary<string, LinkDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["self"] = Hateoas.Link(Url.Action(GetAction, new { version, page = result.Page, pageSize = result.PageSize }) ?? string.Empty, HttpMethod.Get.Method),
            ["create"] = Hateoas.Link(Url.Action(GetAction, new { version }) ?? string.Empty, HttpMethod.Post.Method)
        };

        if (result.HasPrevious)
            links["prev"] = Hateoas.Link(
                Url.Action(GetAction, new { version, page = result.Page - 1, pageSize = result.PageSize }) ?? string.Empty,
                HttpMethod.Get.Method);

        if (result.HasNext)
            links["next"] = Hateoas.Link(
                Url.Action(GetAction, new { version, page = result.Page + 1, pageSize = result.PageSize }) ?? string.Empty,
                HttpMethod.Get.Method);

        return links;
    }
}
