using System.Text.Json;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents the information of an order item.
/// </summary>
public record OrderItemInfo(UnitValue<decimal> Quantity, string? Description, UnitValue<decimal> Weight, JsonElement? MetaData)
{
    /// <summary>
    /// The quantity of the item in the order.
    /// </summary>
    public UnitValue<decimal> Quantity { get; init; } = Quantity;

    /// <summary>
    /// An optional description of the item.
    /// </summary>
    public string? Description { get; init; } = Description;

    /// <summary>
    /// The weight of the item in the order.
    /// </summary>
    public UnitValue<decimal> Weight { get; init; } = Weight;

    /// <summary>
    /// Additional metadata for the item in JSON format.
    /// </summary>
    public JsonElement? MetaData { get; init; } = MetaData;
}