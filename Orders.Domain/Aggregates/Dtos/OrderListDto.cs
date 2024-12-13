namespace Orders.Domain.Aggregates.Dtos;

public class OrderListDto : List<OrderDto>
{
    public OrderListDto(ICollection<OrderDto> orders) : base(orders)
    {
        AddRange(orders);
    }
}