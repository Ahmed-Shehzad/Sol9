using System.Text.Json;
using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities;

public class OrderItem() : BaseEntity(Ulid.NewUlid())
{
    public Ulid OrderId { get; private set; }

    public Order Order { get; private set; }

    public Ulid? ProductId { get; private set; }

    public Ulid? StopItemId { get; private set; }

    public Ulid? TripId { get; private set; }

    public OrderItemInfo OrderItemInfo { get; private set; }

    private OrderItem(Order order,
        Ulid? productId,
        Ulid? stopItemId,
        Ulid? tripId,
        OrderItemInfo orderItemInfo) : this()
    {
        OrderId = order.Id;
        Order = order;
        ProductId = productId;
        StopItemId = stopItemId;
        TripId = tripId;
        OrderItemInfo = orderItemInfo;
    }
    
    public static OrderItem Create(
        Order order,
        Ulid? productId,
        Ulid? stopItemId,
        Ulid? tripId,
        OrderItemInfo orderItemInfo)
    {
        return new OrderItem(order, productId, stopItemId, tripId, orderItemInfo);
    }
}