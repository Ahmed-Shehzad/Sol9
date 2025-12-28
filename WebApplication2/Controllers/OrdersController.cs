using Microsoft.AspNetCore.Mvc;

using WebApplication2.Application.Orders;
using WebApplication2.Domain.Orders;

namespace WebApplication2.Controllers;

/// <summary>
/// Provides endpoints for retrieving orders.
/// </summary>
[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderReadRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="repository">The order read repository.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> is null.</exception>
    public OrdersController(IOrderReadRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Gets all orders asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of <see cref="OrderSummary"/>.</returns>
    /// <response code="200">Returns the list of orders.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderSummary>>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<OrderSummary> orders = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Ok(orders);
    }
}
