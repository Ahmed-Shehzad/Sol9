namespace Orders.Domain.Aggregates.Dtos;

public record OrdersDto
{
    private OrdersDto(OrderListDto orders)
    {
        Orders = orders;
    }
    public static OrdersDto Create(OrderListDto orders)
    {
        return new OrdersDto(orders);
    }

    public OrderListDto Orders { get; init; }
}