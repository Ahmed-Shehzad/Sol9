using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Application.Dtos.Orders;

using Verifier;

namespace Orders.Application.Queries.GetOrders;

public sealed record GetOrdersQuery : IQuery<IReadOnlyList<OrderDto>>;

public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
    }
}

public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IReadOnlyOrdersDbContext _dbContext;

    public GetOrdersQueryHandler(IReadOnlyOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrderDto>> HandleAsync(GetOrdersQuery request, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.Status,
                order.CreatedAtUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
