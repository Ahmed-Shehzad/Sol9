namespace Orders.Domain.Aggregates.Dtos;

public record OrdersDto
{
    private OrdersDto(OrderListDto orders, int nextPageNumber, int nextPageSize)
    {
        Orders = orders;
        NextPageNumber = nextPageNumber;
        NextPageSize = nextPageSize;
    }
    public static OrdersDto Create(OrderListDto orders, int nextPageNumber, int nextPageSize)
    {
        return new OrdersDto(orders, nextPageNumber, nextPageSize);
    }

    public OrderListDto Orders { get; init; }
    public int NextPageNumber { get; init; }
    public int NextPageSize { get; init; }
}