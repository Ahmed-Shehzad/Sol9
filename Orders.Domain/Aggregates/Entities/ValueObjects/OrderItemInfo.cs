using System.Text.Json;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents the information of an order item.
/// </summary>
public record OrderItemInfo
{
    /// <summary>
    /// Represents the information of an order item.
    /// </summary>
    private OrderItemInfo(UnitValue<decimal> quantity, string? description, UnitValue<decimal> weight, JsonElement? metaData)
    {
        Quantity = quantity;
        Description = description;
        Weight = weight;
        MetaData = metaData;
    }
    
    public static OrderItemInfo Create(UnitValue<decimal> quantity, string? description, UnitValue<decimal> weight, JsonElement? metaData)
    {
        return new OrderItemInfo(quantity, description, weight, metaData);
    }

    /// <summary>
    /// The quantity of the item in the order.
    /// </summary>
    public UnitValue<decimal> Quantity { get; init; }

    /// <summary>
    /// An optional description of the item.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The weight of the item in the order.
    /// </summary>
    public UnitValue<decimal> Weight { get; init; }

    /// <summary>
    /// Additional metadata for the item in JSON format.
    /// </summary>
    public JsonElement? MetaData { get; init; }
}