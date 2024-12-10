using MediatR;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents a domain event that is both a notification.
/// </summary>
public interface IDomainEvent : INotification;

/// <summary>
/// Represents a generic domain event associated with a specific command type.
/// </summary>
/// <typeparam name="TCommand">The type of the command associated with this domain event.</typeparam>
public interface IDomainEvent<TCommand> : IDomainEvent;
