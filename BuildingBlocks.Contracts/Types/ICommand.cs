using MediatR;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents a marker interface for command objects in the system.
/// </summary>
/// <remarks>
/// This interface doesn't define any methods or properties. It is used to
/// identify classes that represent commands in the application architecture.
/// </remarks>
public interface ICommand;

/// <summary>
/// Defines a command interface that extends the <see cref="IRequest{T}"/> and <see cref="ICommand"/> interfaces.
/// </summary>
/// <typeparam name="T">The type of the response returned by the command.</typeparam>
/// <remarks>
/// This interface combines the functionality of MediatR's IRequest with the custom ICommand interface.
/// It is used for commands that expect a response of type T.
/// </remarks>
public interface ICommand<out T> : IRequest<T>, ICommand;