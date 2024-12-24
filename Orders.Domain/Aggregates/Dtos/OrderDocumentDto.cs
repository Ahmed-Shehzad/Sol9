using System.Text.Json;
using BuildingBlocks.Domain.Aggregates.Dtos;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Dtos;

public record OrderDocumentDto : BaseDto
{
    private OrderDocumentDto(Ulid Id,
        OrderDocumentInfo DocumentInfo,
        Ulid OrderId,
        Order Order,
        JsonElement? MetaData,
        Ulid? TenantId,
        Ulid? UserId) : base(Id.ToGuid())
    {
        this.DocumentInfo = DocumentInfo;
        this.OrderId = OrderId;
        this.Order = Order;
        this.MetaData = MetaData;
        this.TenantId = TenantId;
        this.UserId = UserId;
    }
    public static OrderDocumentDto Create(Ulid id, OrderDocumentInfo documentInfo, Ulid orderId, Order order, JsonElement? metaData,
        Ulid? tenantId, Ulid? userId)
    {
        return new OrderDocumentDto(id, documentInfo, orderId, order, metaData, tenantId, userId);
    }

    /// <summary>
    /// Gets the details of the document.
    /// </summary>
    public OrderDocumentInfo DocumentInfo { get; init; }

    /// <summary>
    /// Gets the unique identifier of the order associated with the document.
    /// </summary>
    public Ulid OrderId { get; init; }

    /// <summary>
    /// Gets the order associated with the document.
    /// </summary>
    public Order Order { get; init; }

    /// <summary>
    /// Gets the additional metadata for the document.
    /// </summary>
    public JsonElement? MetaData { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant associated with the document.
    /// </summary>
    public Ulid? TenantId { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the document.
    /// </summary>
    public Ulid? UserId { get; init; }
}