using BuildingBlocks.Infrastructure.Types;
using Orders.Application.Dtos;

namespace Orders.Application.Queries.Orders;

public record GetOrdersQuery : QueryBase<OrdersDto>
{
    public GetOrdersQuery(int pageNumber = 1, int pageSize = 100)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 100 : pageSize;
        pageSize = pageSize >= 1000 ? 1000 : pageSize;

        PageNumber = pageNumber;
        PageSize = pageSize;
    }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
}