using System.Text.Json;
using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities;

public class OrderItem() : BaseEntity(Ulid.NewUlid())
{
    public Ulid OrderId { get; private set; }

    public Order Order { get; private set; }

    public Ulid? ProductId { get; private set; }

    public Ulid? StopItemId { get; private set; }

    public Ulid? TripId { get; private set; }

    public UnitValue<decimal> Quantity { get; private set; }

    public string? Description { get; private set; }

    public UnitValue<decimal> Weight { get; private set; }

    public JsonElement? MetaData { get; private set; }

    private OrderItem(Order order,
        Ulid? productId,
        Ulid? stopItemId,
        Ulid? tripId,
        UnitValue<decimal> quantity,
        string? description,
        UnitValue<decimal> weight,
        JsonElement? metaData) : this()
    {
        OrderId = order.Id;
        Order = order;
        ProductId = productId;
        StopItemId = stopItemId;
        TripId = tripId;
        Quantity = quantity;
        Description = description;
        Weight = weight;
        MetaData = metaData;
    }
    public static OrderItem Create(
        Order order,
        Ulid? productId,
        Ulid? stopItemId,
        Ulid? tripId,
        UnitValue<decimal> quantity,
        string? description,
        UnitValue<decimal> weight,
        JsonElement? metaData)
    {
        return new OrderItem(order, productId, stopItemId, tripId, quantity, description, weight, metaData);
    }
}