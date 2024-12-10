using BuildingBlocks.Domain.Types;

namespace Orders.Domain.Aggregates.Enums;

public class OrderStatus : Enumeration
{
    private OrderStatus(int id, string value) : base(id, value)
    {
    }

    public static readonly OrderStatus Draft = new(0, nameof(Draft));
    public static readonly OrderStatus Open = new(1, nameof(Open));
    public static readonly OrderStatus Scheduled = new(2, nameof(Scheduled));
    public static readonly OrderStatus InProgress = new(3, nameof(InProgress));
    public static readonly OrderStatus Finished = new(4, nameof(Finished));
    public static readonly OrderStatus Ignored = new(5, nameof(Ignored));
    public static readonly OrderStatus Error = new(6, nameof(Error));
    public static readonly OrderStatus Skipped = new(7, nameof(Skipped));
}