using Intercessor.Abstractions;

using Microsoft.EntityFrameworkCore;

using Orders.Application.Contexts;
using Orders.Application.Dtos.Orders;

using Verifier;

namespace Orders.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Ulid Id) : IQuery<OrderDto?>;

public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        _ = RuleFor(query => query.Id)
            .Must(id => id != Ulid.Empty, "Id must not be empty.");
    }
}

public sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IReadOnlyOrdersDbContext _dbContext;

    public GetOrderByIdQueryHandler(IReadOnlyOrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDto?> HandleAsync(GetOrderByIdQuery request, CancellationToken cancellationToken = default)
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
