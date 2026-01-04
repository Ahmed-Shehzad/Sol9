using Asp.Versioning;

using Bookings.API.Requests;
using Bookings.Application.Commands.CreateBooking;
using Bookings.Application.Dtos;
using Bookings.Application.Queries.GetBookingByOrderId;
using Bookings.Application.Queries.GetBookings;

using Intercessor.Abstractions;

using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetAsync()
    {
        const string key = nameof(GetBookingsQuery);
        IReadOnlyList<BookingDto> bookings = await _sender.SendAsync(new GetBookingsQuery(key)).ConfigureAwait(false);
        return Ok(bookings);
    }

    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetByOrderIdAsync(Ulid orderId)
    {
        BookingDto? booking = await _sender.SendAsync(new GetBookingByOrderIdQuery(orderId)).ConfigureAwait(false);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BookingDto>> CreateAsync([FromBody] CreateBookingRequest request)
    {
        BookingDto booking = await _sender.SendAsync(new CreateBookingCommand(request.OrderId, request.CustomerName)).ConfigureAwait(false);
        const string action = nameof(GetByOrderIdAsync);
        return CreatedAtAction(action, new { orderId = booking.OrderId }, booking);
    }
}
