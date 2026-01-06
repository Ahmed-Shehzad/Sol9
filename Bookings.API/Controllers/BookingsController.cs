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
using Bookings.API.Responses;

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
    public async Task<ActionResult<CollectionDto<ResourceDto<BookingDto>>>> GetAsync(CancellationToken cancellationToken = default)
    {
        const string key = nameof(GetBookingsQuery);
        IReadOnlyList<BookingDto> bookings = await _sender.SendAsync(new GetBookingsQuery(key), cancellationToken).ConfigureAwait(false);
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        IReadOnlyList<ResourceDto<BookingDto>> items = [.. bookings.Select(booking => new ResourceDto<BookingDto>(booking, BuildBookingLinks(booking.Id, booking.OrderId, version)))];

        return Ok(new CollectionDto<ResourceDto<BookingDto>>(items, BuildCollectionLinks(version)));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto<BookingDto>>> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        BookingDto? order = await _sender.SendAsync(new GetBookingByIdQuery(id.ToUlid()), cancellationToken).ConfigureAwait(false);
        if (order is null) return NotFound();
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return Ok(new ResourceDto<BookingDto>(order, BuildBookingLinks(order.Id, order.OrderId, version)));
    }

    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(ResourceDto<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceDto<BookingDto>>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        BookingDto? booking = await _sender.SendAsync(new GetBookingByOrderIdQuery(orderId.ToUlid()), cancellationToken).ConfigureAwait(false);
        if (booking is null) return NotFound();
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return Ok(new ResourceDto<BookingDto>(booking, BuildBookingLinks(booking.Id, booking.OrderId, version)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<Guid>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceDto<Guid>>> CreateAsync([FromBody] CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        Guid bookingId = await _sender.SendAsync(new CreateBookingCommand(request.OrderId.ToUlid(), request.CustomerName), cancellationToken).ConfigureAwait(false);
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        var resource = new ResourceDto<Guid>(bookingId, BuildBookingLinks(bookingId, request.OrderId, version));
        return CreatedAtAction(GetByIdAction, new { id = bookingId, version }, resource);
    }

    private IReadOnlyDictionary<string, LinkDto> BuildBookingLinks(Guid id, Guid orderId, string? version)
        => new Dictionary<string, LinkDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["self"] = new LinkDto(Url.Action(GetByIdAction, new { id, version }) ?? string.Empty, "GET"),
            ["byOrder"] = new LinkDto(Url.Action(GetByOrderAction, new { orderId, version }) ?? string.Empty, "GET"),
            ["collection"] = new LinkDto(Url.Action(GetAction, new { version }) ?? string.Empty, "GET")
        };

    private IReadOnlyDictionary<string, LinkDto> BuildCollectionLinks(string? version)
        => new Dictionary<string, LinkDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["self"] = new LinkDto(Url.Action(GetAction, new { version }) ?? string.Empty, "GET"),
            ["create"] = new LinkDto(Url.Action(GetAction, new { version }) ?? string.Empty, "POST")
        };
}
