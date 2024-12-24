using Asp.Versioning;
using BuildingBlocks.Utilities.Types;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Orders.Application.Queries.Orders;
using Orders.Domain.Aggregates.Dtos;
using Treblle.Net.Core;

namespace Orders.API.Controllers;

[ApiVersion("1.0")]
[Treblle]
public class OrdersController(ILogger<OrdersController> logger, IMediator mediator, IDistributedCache distributedCache)
    : ApiControllerBase(mediator)
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<OrdersController> _logger = logger;
    private readonly IDistributedCache _distributedCache = distributedCache;

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(OrdersDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OrdersDto>> GetOrdersAsync()
    {
        var query = new GetOrdersQuery();
        return NotNull(await QueryAsync(query));
    }
}