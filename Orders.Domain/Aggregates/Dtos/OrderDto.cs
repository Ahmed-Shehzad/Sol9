using BuildingBlocks.Domain.Aggregates.Dtos;
using Orders.Domain.Aggregates.Entities.Enums;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Dtos;

public record OrderDto : BaseDto
{
    private OrderDto(Ulid Id,
        string Type,
        string Description,
        OrderStatus Status,
        OrderAddressDto? Address,
        ICollection<OrderTimeFrame> TimeFrames,
        ICollection<OrderItemDto> Items,
        ICollection<OrderDocumentDto> Documents,
        ICollection<Depot> Depots,
        Ulid? TenantId,
        Ulid? UserId) : base(Id)
    {
        this.Type = Type;
        this.Description = Description;
        this.Status = Status;
        this.Address = Address;
        this.TimeFrames = TimeFrames;
        this.Items = Items;
        this.Documents = Documents;
        this.Depots = Depots;
        this.TenantId = TenantId;
        this.UserId = UserId;
    }
    public static OrderDto Create(Ulid id, string type, string description, OrderStatus status, OrderAddressDto? address,
        ICollection<OrderTimeFrame> timeFrames, ICollection<OrderItemDto> items, ICollection<OrderDocumentDto> documents,
        ICollection<Depot> depots, Ulid? tenantId, Ulid? userId)
    {
        return new OrderDto(id, type, description, status, address, timeFrames, items, documents, depots, tenantId, userId);
    }

    /// <summary>
    /// Gets the type of the order.
    /// </summary>
    public string Type { get; init; }

    /// <summary>
    /// Gets the description of the order.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets the status of the order.
    /// </summary>
    public OrderStatus Status { get; init; }

    /// <summary>
    /// Gets the address information associated with the order.
    /// </summary>
    public OrderAddressDto? Address { get; init; }

    /// <summary>
    /// Gets a collection of time frames associated with the order.
    /// </summary>
    public ICollection<OrderTimeFrame> TimeFrames { get; init; }

    /// <summary>
    /// Gets a collection of order items associated with the order.
    /// </summary>
    public ICollection<OrderItemDto> Items { get; init; }

    /// <summary>
    /// Gets a collection of documents associated with the order.
    /// </summary>
    public ICollection<OrderDocumentDto> Documents { get; init; }

    /// <summary>
    /// Gets a collection of depots associated with the order.
    /// </summary>
    public ICollection<Depot> Depots { get; init; }

    /// <summary>
    /// Gets the unique identifier of the tenant associated with the order.
    /// </summary>
    public Ulid? TenantId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user associated with the order.
    /// </summary>
    public Ulid? UserId { get; init; }
}