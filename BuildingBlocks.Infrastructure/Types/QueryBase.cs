using BuildingBlocks.Contracts.Types;
using FluentResults;

namespace BuildingBlocks.Infrastructure.Types;

/// <summary>
/// Query base class
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract record QueryBase<TResult> : IQuery<TResult>;

/// <summary>
/// Query base class with result
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract record QueryBaseResult<TResult> : IQuery<Result<TResult>>;