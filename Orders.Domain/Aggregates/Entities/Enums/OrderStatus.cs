using BuildingBlocks.Utilities.Types;

namespace Orders.Domain.Aggregates.Entities.Enums;

/// <summary>
/// Represents the different statuses an order can have.
/// </summary>
public class OrderStatus : Enumeration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderStatus"/> class.
    /// </summary>
    /// <param name="key">The unique identifier of the order status.</param>
    /// <param name="value">The display name of the order status.</param>
    private OrderStatus(int key, string value) : base(key, value)
    {
    }

    /// <summary>
    /// Represents an order that is still in draft mode and not ready for processing.
    /// </summary>
    public static readonly OrderStatus Draft = new(0, nameof(Draft));

    /// <summary>
    /// Represents an order that is open and ready for processing.
    /// </summary>
    public static readonly OrderStatus Open = new(1, nameof(Open));

    /// <summary>
    /// Represents an order that has been scheduled for future processing.
    /// </summary>
    public static readonly OrderStatus Scheduled = new(2, nameof(Scheduled));

    /// <summary>
    /// Represents an order that is currently being processed.
    /// </summary>
    public static readonly OrderStatus InProgress = new(3, nameof(InProgress));

    /// <summary>
    /// Represents an order that has been completed successfully.
    /// </summary>
    public static readonly OrderStatus Finished = new(4, nameof(Finished));

    /// <summary>
    /// Represents an order that should be ignored for processing.
    /// </summary>
    public static readonly OrderStatus Ignored = new(5, nameof(Ignored));

    /// <summary>
    /// Represents an order that encountered an error during processing.
    /// </summary>
    public static readonly OrderStatus Error = new(6, nameof(Error));

    /// <summary>
    /// Represents an order that has been skipped during processing.
    /// </summary>
    public static readonly OrderStatus Skipped = new(7, nameof(Skipped));
}