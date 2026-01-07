using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Application.Dtos.Orders;

using Sol9.Core.Pagination;

using Verifier;

namespace Orders.Application.Queries.GetOrders;

public sealed record GetOrdersQuery(
    string CacheKey,
    int Page,
    int PageSize,
    TimeSpan? CacheDuration = null) : ICachedQuery<PagedResult<OrderDto>>;

public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        _ = RuleFor(request => request.Page)
            .Must(page => page >= 1, "Page must be greater than or equal to 1.");
        _ = RuleFor(request => request.PageSize)
            .Must(
                size => size >= 1 && size <= PaginationRequest.MaxPageSize,
                $"PageSize must be between 1 and {PaginationRequest.MaxPageSize}.");
    }
}

public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IReadOnlyOrdersDbContext _dbContext;

    public GetOrdersQueryHandler(IReadOnlyOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<OrderDto>> HandleAsync(GetOrdersQuery request, CancellationToken cancellationToken = default)
    {
        IQueryable<Orders.Domain.Entities.Order> query = _dbContext.Orders
            .OrderByDescending(order => order.CreatedAtUtc)
            .AsQueryable();

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        IReadOnlyList<OrderDto> items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(order => new OrderDto(
                order.Id.ToGuid(),
                order.CustomerName,
                order.TotalAmount,
                order.Status,
                order.CreatedAtUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<OrderDto>(items, totalCount, request.Page, request.PageSize);
    }
}
