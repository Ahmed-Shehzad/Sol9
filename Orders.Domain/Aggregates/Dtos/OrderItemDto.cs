using BuildingBlocks.Domain.Aggregates.Dtos;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Dtos;

public record OrderItemDto : BaseDto
{
    private OrderItemDto(Ulid Id, Ulid OrderId, Order Order, Ulid? ProductId, Ulid? StopItemId, Ulid? TripId,
        OrderItemInfo OrderItemInfo) : base(Id)
    {
        this.OrderId = OrderId;
        this.Order = Order;
        this.ProductId = ProductId;
        this.StopItemId = StopItemId;
        this.TripId = TripId;
        this.OrderItemInfo = OrderItemInfo;
    }
    public static OrderItemDto Create(Ulid id, Ulid orderId, Order order, Ulid? productId, Ulid? stopItemId, Ulid? tripId,
        OrderItemInfo orderItemInfo)
    {
        return new OrderItemDto(id, orderId, order, productId, stopItemId, tripId, orderItemInfo);
    }

    /// <summary>
    /// Gets the unique identifier of the order.
    /// </summary>
    public Ulid OrderId { get; init; }

    /// <summary>
    /// Gets the order associated with this order item.
    /// </summary>
    public Order Order { get; init; }

    /// <summary>
    /// Gets the unique identifier of the product associated with this order item.
    /// </summary>
    public Ulid? ProductId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the stop item associated with this order item.
    /// </summary>
    public Ulid? StopItemId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the trip associated with this order item.
    /// </summary>
    public Ulid? TripId { get; init; }

    /// <summary>
    /// Gets the information about the order item.
    /// </summary>
    public OrderItemInfo OrderItemInfo { get; init; }
}