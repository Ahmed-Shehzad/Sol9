using BuildingBlocks.Contracts.Types;
using FluentResults;

namespace BuildingBlocks.Infrastructure.Types;

/// <summary>
/// Command base Result class
/// </summary>
/// <typeparam name="T"></typeparam>
public record CommandBaseResult<T> : ICommand<Result<T>>;

/// <summary>
/// Command base class
/// </summary>
/// <typeparam name="T"></typeparam>
public record CommandBase<T> : ICommand<T>;