using Intercessor.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TestApi.Commands;
using TestApi.Queries;

namespace TestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly ISender _sender;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        await _sender.SendAsync(new CreateUserCommand("hello"));
        var update = await _sender.SendAsync(new UpdateUserCommand( Guid.NewGuid(), "world"));
        var user = await _sender.SendAsync(new GetUserQuery("", TimeSpan.FromSeconds(10)));

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}