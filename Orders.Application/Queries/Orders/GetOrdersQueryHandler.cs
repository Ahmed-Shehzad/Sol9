using BuildingBlocks.Contracts.Types;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orders.Domain.Aggregates.Dtos;
using Orders.Domain.Aggregates.Mappings;
using Orders.Infrastructure.Contexts.Contracts;

namespace Orders.Application.Queries.Orders;

public class GetOrdersQueryHandler(ILogger<GetOrdersQueryHandler> logger, IOrdersReadOnlyDbContext ordersReadOnlyDbContext)
    : IQueryHandler<GetOrdersQuery, Result<OrdersDto>>
{
    private readonly ILogger _logger = logger;
    public async Task<Result<OrdersDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetOrdersQueryHandler: Got a request to get Orders with PageNumber: {PageNumber} and PageSize: {PageSize}",
            request.PageNumber,
            request.PageSize);

        var query = ordersReadOnlyDbContext.Orders.OrderByDescending(o => o.CreatedDateUtcAt)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);

#if DEBUG
        _logger.LogInformation("GetOrdersQueryHandler: Query: {Query}", query.ToQueryString());
#endif

        var orders = await query.ToListAsync(cancellationToken);
        var orderListDto = orders.MapOrdersToDto();

        var ordersDto = OrdersDto.Create(new OrderListDto(orderListDto), request.PageNumber, request.PageSize);
        return Result.Ok(ordersDto);
    }
}