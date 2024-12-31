namespace Orders.Application.Dtos;

public class OrdersDto : List<OrderDto>
{
    public OrdersDto(List<OrderDto> orders) : base(orders)
    {
        AddRange(orders);
    }
}