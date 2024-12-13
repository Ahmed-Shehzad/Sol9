using BuildingBlocks.Utilities.Types;

namespace Inventories.Domain.Aggregates.Entities.Enums;

/// <summary>
/// Represents the status of a product in the inventory system.
/// </summary>
public class ProductStatus : Enumeration
{
    public ProductStatus() : base()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductStatus"/> class.
    /// </summary>
    /// <param name="key">The unique identifier of the product status.</param>
    /// <param name="value">The display name of the product status.</param>
    private ProductStatus(int key, string value) : base(key, value)
    {
    }

    /// <summary>
    /// Represents an inactive product status.
    /// </summary>
    public static readonly ProductStatus Inactive = new(0, nameof(Inactive));

    /// <summary>
    /// Represents an active product status.
    /// </summary>
    public static readonly ProductStatus Active = new(1, nameof(Active));
}