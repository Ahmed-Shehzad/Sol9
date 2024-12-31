using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Orders.Application.Dtos;
using Orders.Application.Mappings;
using Orders.Infrastructure.Contexts.Contracts;

namespace Orders.Application.Queries.Orders;

public class GetOrdersQueryHandler(
    ILogger<GetOrdersQueryHandler> logger,
    IOrdersReadOnlyDbContext ordersReadOnlyDbContext,
    IDistributedCache distributedCache)
    : IQueryHandler<GetOrdersQuery, OrdersDto>
{
    public async Task<OrdersDto> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetOrdersAsync: {Query}", request);

        var key = $"{nameof(GetOrdersQuery)}-{request.PageNumber}-{request.PageSize}";

        var orders = await distributedCache.GetOrSetAsync(key, async () =>
        {
            logger.LogInformation(
                "GetOrdersQueryHandler: Got a request to get Orders with PageNumber: {PageNumber} and PageSize: {PageSize}",
                request.PageNumber,
                request.PageSize);

            var query = ordersReadOnlyDbContext.Orders
                .OrderByDescending(o => o.CreatedDateUtcAt) // Order by CreatedDateUtcAt first
                .ThenByDescending(o => o.CreatedTimeUtcAt) // Then order by CreatedTimeUtcAt
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

#if DEBUG
            logger.LogInformation("GetOrdersQueryHandler: Query: {Query}", query.ToQueryString());
#endif

            var response = await query.ToListAsync(cancellationToken);
            return response.MapOrdersToDto();
        });
        
        return new OrdersDto(orders ?? []);
    }
}