using BuildingBlocks.Contracts.Types;
using FluentResults;

namespace BuildingBlocks.Infrastructure.Types;

/// <summary>
/// Command base class
/// </summary>
/// <typeparam name="T"></typeparam>
public record CommandBase<T> : ICommand<Result<T>>;