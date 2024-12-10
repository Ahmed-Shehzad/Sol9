using System.Collections.ObjectModel;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using Orders.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.ValueObjects;
using Orders.Domain.Aggregates.Enums;

namespace Orders.Domain.Aggregates;

public class Order : AggregateRoot, ITenantDependent, IUserDependent
{
    private Order(string type,
        string description,
        OrderStatus status,
        Address billingAddress,
        Address shippingAddress,
        Address transportAddress)
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
        Address billingAddress,
        Address shippingAddress,
        Address transportAddress)
    {
        return new Order(type,
            description,
            status,
            billingAddress,
            shippingAddress,
            transportAddress);
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
    }
    public void UpdateBillingAddress(Address  billingAddress)
    {
        BillingAddress = billingAddress;
    }
    public void UpdateShippingAddress(Address shippingAddress)
    {
        ShippingAddress = shippingAddress;
    }
    public void UpdateTransportAddress(Address transportAddress)
    {
        TransportAddress = transportAddress;
    }
    public List<OrderItem> GetOrderItemsByProduct(Ulid productId)
    {
        return Items.Where(x => x.ProductId == productId).ToList();
    }
    public List<OrderItem> GetOrderItemsByStopItem(Ulid stopItemId)
    {
        return Items.Where(x => x.StopItemId == stopItemId).ToList();
    }
    public List<OrderItem> GetOrderItemsByTrip(Ulid tripId)
    {
        return Items.Where(x => x.TripId == tripId).ToList();
    }
    public long GetTotalQuantity()
    {
        var quantity = Items.LongCount(x => x.Quantity.Value > 0);
        return quantity;
    }
    
    public long GetTotalWeight()
    {
        var weight = Items.LongCount(x => x.Weight.Value > 0);
        return weight;
    }
    public void UpdateDescription(string description)
    {
        Description = description;
    }
    public void AddDocument(Document document)
    {
        Documents.Add(document);
    }
    public void RemoveDocument(Document document)
    {
        Documents.Remove(document);
    }
    public void AddDepot(Depot depot)
    {
        Depots.Add(depot);
    }
    public void RemoveDepot(Depot depot)
    {
        Depots.Remove(depot);
    }

    public string Type { get; private set; }

    public string Description { get; private set; }

    public OrderStatus Status { get; private set; }

    public Address BillingAddress { get; private set; }

    public Address ShippingAddress { get; private set; }

    public Address TransportAddress { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new Collection<OrderItem>();

    public ICollection<Document> Documents { get; private set; } = new Collection<Document>();

    public ICollection<Depot> Depots { get; private set; } = new Collection<Depot>();

    public Ulid? TenantId { get; set; }
    
    public Ulid? UserId { get; set; }
}