using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities;

public class OrderDocument : BaseEntity, ITenantDependent, IUserDependent
{
    public OrderDocument() : base(Ulid.NewUlid())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderDocument"/> class.
    /// </summary>
    /// <param name="documentInfo">The details of the document.</param>
    /// <param name="order">The order associated with the document.</param>
    /// <param name="metaData">Additional metadata for the document.</param>
    private OrderDocument(OrderDocumentInfo documentInfo, Order order, JsonElement? metaData) : this()
    {
        DocumentInfo = documentInfo;
        OrderId = order.Id;
        Order = order;
        MetaData = metaData;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="OrderDocument"/> class.
    /// </summary>
    /// <param name="documentInfo">The details of the document.</param>
    /// <param name="order">The order associated with the document.</param>
    /// <param name="metaData">Additional metadata for the document.</param>
    /// <returns>A new instance of the <see cref="OrderDocument"/> class.</returns>
    public static OrderDocument Create(OrderDocumentInfo documentInfo, Order order, JsonElement? metaData)
    {
        return new OrderDocument(documentInfo, order, metaData);
    }

    /// <summary>
    /// Updates the document with new details.
    /// </summary>
    /// <param name="documentInfo">The updated details of the document.</param>
    /// <param name="order">The updated order associated with the document.</param>
    /// <param name="metaData">The updated additional metadata for the document.</param>
    public void Update(OrderDocumentInfo documentInfo, Order order, JsonElement? metaData)
    {
        DocumentInfo = documentInfo;
        OrderId = order.Id;
        Order = order;
        MetaData = metaData;
    }

    public void UpdateTenantId(Ulid tenantId)
    {
        TenantId = tenantId;
    }
    public void UpdateUserId(Ulid userId)
    {
        UserId = userId;
    }

    /// <summary>
    /// Gets the details of the document.
    /// </summary>
    public OrderDocumentInfo DocumentInfo { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the order associated with the document.
    /// </summary>
    public Ulid OrderId { get; private set; }

    /// <summary>
    /// Gets the order associated with the document.
    /// </summary>
    public Order Order { get; private set; }

    /// <summary>
    /// Gets the additional metadata for the document.
    /// </summary>
    public JsonElement? MetaData { get; private set; }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant associated with the document.
    /// </summary>
    public Ulid? TenantId { get; private set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the document.
    /// </summary>
    public Ulid? UserId { get; private set; }
}