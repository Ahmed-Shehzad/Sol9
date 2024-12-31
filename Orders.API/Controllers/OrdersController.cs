using Asp.Versioning;
using BuildingBlocks.Utilities.Types;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Dtos;
using Orders.Application.Queries.Orders;

namespace Orders.API.Controllers;

/// <summary>
/// Orders Endpoints
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[ApiVersion("2.0")]
// [Treblle]
public class OrdersController(IMediator mediator)
    : ApiControllerBase(mediator)
{
    [HttpGet("")]
    [MapToApiVersion("1.0")]
    [Obsolete]
    public Task<IActionResult> Get()
    {
        return Task.FromResult<IActionResult>(Ok("Orders API"));
    }

    /// <summary>
    /// Get Orders
    /// </summary>
    /// <returns>Orders information</returns>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(OrdersDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<OrdersDto>> GetOrdersAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var query = new GetOrdersQuery(pageNumber, pageSize);
        var response = await QueryAsync(query);
        return NotNull(response);
    }
}