using MediatR;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents a command handler for commands that don't return a value.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand> 
    where TCommand : ICommand<Unit>, IRequest;

/// <summary>
/// Represents a command handler for commands that return a response.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> 
    where TCommand : ICommand<TResponse>;