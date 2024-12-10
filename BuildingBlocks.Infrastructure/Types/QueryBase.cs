using BuildingBlocks.Contracts.Types;
using FluentResults;

namespace BuildingBlocks.Infrastructure.Types;

/// <summary>
/// Query base class
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract record QueryBase<TResult> : IQuery<Result<TResult>>;