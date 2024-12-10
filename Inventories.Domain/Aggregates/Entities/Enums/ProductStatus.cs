using BuildingBlocks.Domain.Types;

namespace Inventories.Domain.Aggregates.Entities.Enums;

public class ProductStatus : Enumeration
{
    private ProductStatus(int id, string value) : base(id, value)
    {
    }

    public static readonly ProductStatus Active = new ProductStatus(1, nameof(Active));
    public static readonly ProductStatus Inactive = new ProductStatus(2, nameof(Inactive));
}