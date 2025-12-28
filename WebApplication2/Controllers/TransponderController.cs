using Microsoft.AspNetCore.Mvc;

using Transponder.Abstractions;
using Transponder.Samples;

namespace WebApplication2.Controllers;

[ApiController]
[Route("transponder")]
public sealed class TransponderController : ControllerBase
{
    private readonly IClientFactory _clients;
    private readonly IHostEnvironment _environment;

    public TransponderController(IClientFactory clients, IHostEnvironment environment)
    {
        _clients = clients ?? throw new ArgumentNullException(nameof(clients));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    [HttpGet("ping")]
    public async Task<ActionResult<PingResponse>> PingAsync([FromQuery] string? message, CancellationToken cancellationToken)
    {
        var request = new PingRequest(
            message ?? $"Hello from {_environment.ApplicationName}",
            _environment.ApplicationName,
            DateTimeOffset.UtcNow);

        IRequestClient<PingRequest> client = _clients.CreateRequestClient<PingRequest>(TimeSpan.FromSeconds(10));
        PingResponse response = await client.GetResponseAsync<PingResponse>(request, cancellationToken);

        return Ok(response);
    }
}
