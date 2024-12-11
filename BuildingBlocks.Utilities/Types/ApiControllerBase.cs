using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Utilities.Types;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ApiControllerBase : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiControllerBase"/> class.
    /// </summary>
    /// <param name="mediator">An instance of <see cref="IMediator"/> for handling requests.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediator"/> is null.</exception>
    public ApiControllerBase(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Asynchronously sends a query request to the mediator and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query request.</param>
    /// <returns>A task that represents the asynchronous operation, returning the result of the query.</returns>
    protected async Task<TResult> QueryAsync<TResult>(IRequest<TResult> query)
    {
        return await _mediator.Send(query);
    }

    /// <summary>
    /// Checks the result of a FluentResults operation and returns an appropriate ActionResult.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="data">The FluentResults result.</param>
    /// <returns>An ActionResult with the result value if successful, or a NotFound result with reasons if failed.</returns>
    protected ActionResult<T> NotNull<T>(Result<T> data)
    {
        return data.IsFailed ? NotFound(data.Reasons) : Ok(data.ValueOrDefault);
    }

    /// <summary>
    /// Asynchronously sends a command request to the mediator and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="command">The command request.</param>
    /// <returns>A task that represents the asynchronous operation, returning the result of the command.</returns>
    protected async Task<TResult> CommandAsync<TResult>(IRequest<TResult> command)
    {
        return await _mediator.Send(command);
    }
}