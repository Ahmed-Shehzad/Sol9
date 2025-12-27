using Microsoft.AspNetCore.Mvc;
using WebApplication2.Application.Orders;
using WebApplication2.Domain.Orders;

namespace WebApplication2.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderReadRepository _repository;

    public OrdersController(IOrderReadRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderSummary>>> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<OrderSummary> orders = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Ok(orders);
    }
}
