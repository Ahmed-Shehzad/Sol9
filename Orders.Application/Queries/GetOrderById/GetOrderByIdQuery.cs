using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

using Orders.Application.Contracts;
using Orders.Application.Dtos.Orders;

namespace Orders.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto?>;

public sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IReadOnlyOrdersDbContext _dbContext;

    public GetOrderByIdQueryHandler(IReadOnlyOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto?> HandleAsync(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .Where(order => order.Id == request.Id)
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.Status,
                order.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
