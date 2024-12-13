using System.Collections.ObjectModel;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using Orders.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.Enums;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates;

public class Order : AggregateRoot, ITenantDependent, IUserDependent
{
    public Order()
    {
    }
    
    private Order(string type,
        string description,
        OrderStatus status,
        Address? billingAddress,
        Address? shippingAddress,
        Address? transportAddress)
    {
        Type = type;
        Description = description;
        Status = status;
        BillingAddress = billingAddress;
        ShippingAddress = shippingAddress;
        TransportAddress = transportAddress;
    }
    
    public static Order Create(string type,
        string description,
        OrderStatus status,
        Address? billingAddress,
        Address? shippingAddress,
        Address? transportAddress)
    {
        return new Order(type,
            description,
            status,
            billingAddress,
            shippingAddress,
            transportAddress);
    }

    /// <summary>
    /// Updates the status of the order.
    /// </summary>
    /// <param name="status">The new status to set for the order.</param>
    /// <remarks>
    /// This method changes the status of the order to the provided status.
    /// </remarks>
    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
    }
    
    /// <summary>
    /// Updates the billing address of the order.
    /// </summary>
    /// <param name="billingAddress">The new billing address to set for the order.</param>
    public void UpdateBillingAddress(Address? billingAddress)
    {
        BillingAddress = billingAddress;
    }

    /// <summary>
    /// Updates the shipping address of the order.
    /// </summary>
    /// <param name="shippingAddress">The new shipping address to set for the order.</param>
    public void UpdateShippingAddress(Address? shippingAddress)
    {
        ShippingAddress = shippingAddress;
    }

    /// <summary>
    /// Updates the transport address of the order.
    /// </summary>
    /// <param name="transportAddress">The new transport address to set for the order.</param>
    public void UpdateTransportAddress(Address? transportAddress)
    {
        TransportAddress = transportAddress;
    }

    /// <summary>
    /// Retrieves a list of order items associated with a specific product.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <returns>A list of order items associated with the specified product.</returns>
    public List<OrderItem> GetOrderItemsByProduct(Ulid productId)
    {
        return Items.Where(x => Equals(x.ProductId, productId)).ToList();
    }

    /// <summary>
    /// Retrieves a list of order items associated with a specific stop item.
    /// </summary>
    /// <param name="stopItemId">The unique identifier of the stop item.</param>
    /// <returns>A list of order items associated with the specified stop item.</returns>
    public List<OrderItem> GetOrderItemsByStopItem(Ulid stopItemId)
    {
        return Items.Where(x => Equals(x.StopItemId, stopItemId)).ToList();
    }

    /// <summary>
    /// Retrieves a list of order items associated with a specific trip.
    /// </summary>
    /// <param name="tripId">The unique identifier of the trip.</param>
    /// <returns>A list of order items associated with the specified trip.</returns>
    public List<OrderItem> GetOrderItemsByTrip(Ulid tripId)
    {
        return Items.Where(x => Equals(x.TripId, tripId)).ToList();
    }

    /// <summary>
    /// Calculates the total quantity of items in the order.
    /// </summary>
    /// <returns>The total quantity of items in the order.</returns>
    public decimal GetTotalQuantity()
    {
        var quantity = Items.Sum(x => x.OrderItemInfo.Quantity.Value);
        return quantity;
    }

    /// <summary>
    /// Calculates the total weight of items in the order.
    /// </summary>
    /// <returns>The total weight of items in the order.</returns>
    public decimal GetTotalWeight()
    {
        var weight = Items.Sum(x => x.OrderItemInfo.Weight.Value);
        return weight;
    }

    /// <summary>
    /// Updates the description of the order.
    /// </summary>
    /// <param name="description">The new description to set for the order.</param>
    public void UpdateDescription(string description)
    {
        Description = description;
    }

    /// <summary>
    /// Adds a document to the order.
    /// </summary>
    /// <param name="document">The document to add to the order.</param>
    public void AddDocument(OrderDocument document)
    {
        Documents.Add(document);
    }

    /// <summary>
    /// Removes a document from the order.
    /// </summary>
    /// <param name="document">The document to remove from the order.</param>
    public void RemoveDocument(OrderDocument document)
    {
        Documents.Remove(document);
    }

    /// <summary>
    /// Adds a depot to the order.
    /// </summary>
    /// <param name="depot">The depot to add to the order.</param>
    public void AddDepot(Depot depot)
    {
        Depots.Add(depot);
    }

    /// <summary>
    /// Removes a depot from the order.
    /// </summary>
    /// <param name="depot">The depot to remove from the order.</param>
    public void RemoveDepot(Depot depot)
    {
        Depots.Remove(depot);
    }
    
    /// <summary>
    /// Adds an order item to the order.
    /// </summary>
    /// <param name="orderItem">The order item to add to the order.</param>
    public void AddOrderItem(OrderItem orderItem)
    {
        Items.Add(orderItem);
    }
    public void RemoveOrderItem(OrderItem orderItem)
    {
        Items.Remove(orderItem);
    }
    public void AddTimeFrame(OrderTimeFrame timeFrame)
    {
        TimeFrames.Add(timeFrame);
    }
    public void RemoveTimeFrame(OrderTimeFrame timeFrame)
    {
        TimeFrames.Remove(timeFrame);
    }

    /// <summary>
    /// Gets or sets the type of the order.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets or sets the description of the order.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets or sets the status of the order.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Gets or sets the billing address of the order.
    /// </summary>
    public Address? BillingAddress { get; private set; }

    /// <summary>
    /// Gets or sets the shipping address of the order.
    /// </summary>
    public Address? ShippingAddress { get; private set; }
    
    /// <summary>
    /// Gets or sets the transport address of the order.
    /// </summary>
    public Address? TransportAddress { get; private set; }

    /// <summary>
    /// Gets a collection of time frames associated with the order.
    /// </summary>
    public ICollection<OrderTimeFrame> TimeFrames { get; private set; } = new Collection<OrderTimeFrame>();
    
    /// <summary>
    /// Gets a collection of order items associated with the order.
    /// </summary>
    public ICollection<OrderItem> Items { get; private set; } = new Collection<OrderItem>();

    /// <summary>
    /// Gets a collection of documents associated with the order.
    /// </summary>
    public ICollection<OrderDocument> Documents { get; private set; } = new Collection<OrderDocument>();

    /// <summary>
    /// Gets a collection of depots associated with the order.
    /// </summary>
    public ICollection<Depot> Depots { get; private set; } = new Collection<Depot>();

    /// <summary>
    /// Gets or sets the unique identifier of the tenant associated with the order.
    /// </summary>
    public Ulid? TenantId { get; private set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the order.
    /// </summary>
    public Ulid? UserId { get; private set; }
}