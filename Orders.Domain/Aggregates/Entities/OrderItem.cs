using BuildingBlocks.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities;

/// <summary>
/// Represents an item in an order.
/// </summary>
public class OrderItem : BaseEntity
{

    /// <summary>
    /// Gets the unique identifier of the order.
    /// </summary>
    public Ulid OrderId { get; private set; }

    /// <summary>
    /// Gets the order associated with this order item.
    /// </summary>
    public Order Order { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the product associated with this order item.
    /// </summary>
    public Ulid? ProductId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the stop item associated with this order item.
    /// </summary>
    public Ulid? StopItemId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the trip associated with this order item.
    /// </summary>
    public Ulid? TripId { get; private set; }

    /// <summary>
    /// Gets the information about the order item.
    /// </summary>
    public OrderItemInfo OrderItemInfo { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="OrderItem"/> class.
    /// </summary>
    /// <param name="order">The order associated with this order item.</param>
    /// <param name="productId">The unique identifier of the product associated with this order item.</param>
    /// <param name="stopItemId">The unique identifier of the stop item associated with this order item.</param>
    /// <param name="tripId">The unique identifier of the trip associated with this order item.</param>
    /// <param name="orderItemInfo">The information about the order item.</param>
    /// <returns>A new instance of the <see cref="OrderItem"/> class.</returns>
    public static OrderItem Create(
        Order order,
        Ulid? productId,
        Ulid? stopItemId,
        Ulid? tripId,
        OrderItemInfo orderItemInfo)
    {
        return new OrderItem(order, productId, stopItemId, tripId, orderItemInfo);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderItem"/> class.
    /// </summary>
    /// <param name="order">The order associated with this order item.</param>
    /// <param name="productId">The unique identifier of the product associated with this order item. Can be null.</param>
    /// <param name="stopItemId">The unique identifier of the stop item associated with this order item. Can be null.</param>
    /// <param name="tripId">The unique identifier of the trip associated with this order item. Can be null.</param>
    /// <param name="orderItemInfo">The information about the order item.</param>
    private OrderItem(Order order,
        Ulid? productId,
        Ulid? stopItemId,
        Ulid? tripId,
        OrderItemInfo orderItemInfo) : base(Ulid.NewUlid())
    {
        OrderId = order.Id;
        Order = order;
        ProductId = productId;
        StopItemId = stopItemId;
        TripId = tripId;
        OrderItemInfo = orderItemInfo;
    }
}