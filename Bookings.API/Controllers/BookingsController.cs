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

namespace Bookings.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/bookings")]
public class BookingsController : ControllerBase
{
    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetAsync(CancellationToken cancellationToken = default)
    {
        const string key = nameof(GetBookingsQuery);
        IReadOnlyList<BookingDto> bookings = await _sender.SendAsync(new GetBookingsQuery(key), cancellationToken).ConfigureAwait(false);
        return Ok(bookings);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        BookingDto? order = await _sender.SendAsync(new GetBookingByIdQuery(id.ToUlid()), cancellationToken).ConfigureAwait(false);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        BookingDto? booking = await _sender.SendAsync(new GetBookingByOrderIdQuery(orderId.ToUlid()), cancellationToken).ConfigureAwait(false);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateAsync([FromBody] CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        Guid bookingId = await _sender.SendAsync(new CreateBookingCommand(request.OrderId.ToUlid(), request.CustomerName), cancellationToken).ConfigureAwait(false);
        const string action = "GetById";
        string? version = HttpContext.GetRequestedApiVersion()?.ToString();
        return CreatedAtAction(action, new { id = bookingId, version }, bookingId);
    }
}
