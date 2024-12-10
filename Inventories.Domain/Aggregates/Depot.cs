using System.Collections.ObjectModel;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using BuildingBlocks.Extensions.Types;
using Inventories.Domain.Aggregates.Entities;
using Inventories.Domain.Aggregates.Entities.ValueObjects;

namespace Inventories.Domain.Aggregates;

public class Depot : AggregateRoot, ITenantDependent, IUserDependent
{
    private Depot(string name, Address address)
    {
        Name = name;
        Address = address;
    }
    public static Depot Create(string name, Address address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(address);
        return new Depot(name, address);
    }
    
    public void Update(string name, Address address)
    {
        Name = name;
        Address = address;
    }
    
    public void AddOperatingHour(OperatingHour operatingHour)
    {
        OperatingHours.Add(operatingHour);
    }
    
    public void AddOperatingHours(ICollection<OperatingHour> operatingHours)
    {
        OperatingHours.AddRange(operatingHours);
    }
    
    public void RemoveOperatingHour(OperatingHour operatingHour)
    {
        OperatingHours.Remove(operatingHour);
    }
    public void RemoveOperatingHours(ICollection<OperatingHour> operatingHours)
    {
        OperatingHours.RemoveRange(operatingHours);
    }
    
    public void AddStockItem(StockItem stockItem)
    {
        StockItems.Add(stockItem);
    }
    public void AddStockItems(ICollection<StockItem> stockItems)
    {
        StockItems.AddRange(stockItems);
    }
    
    public void RemoveStockItem(StockItem stockItem)
    {
        StockItems.Remove(stockItem);
    }
    public void RemoveStockItems(ICollection<StockItem> stockItems)
    {
        StockItems.RemoveRange(stockItems);
    }
    public void AddStockItemBooking(StockItemBooking stockItemBooking)
    {
        StockItemBookings.Add(stockItemBooking);
    }
    public void AddStockItemBookings(ICollection<StockItemBooking> stockItemBookings)
    {
        StockItemBookings.AddRange(stockItemBookings);
    }
    public void RemoveStockItemBooking(StockItemBooking stockItemBooking)
    {
        StockItemBookings.Remove(stockItemBooking);
    }
    public void RemoveStockItemBookings(ICollection<StockItemBooking> stockItemBookings)
    {
        StockItemBookings.RemoveRange(stockItemBookings);
    }

    public string Name { get; private set; }

    public Address Address { get; private set; }

    public ICollection<OperatingHour> OperatingHours { get; private set; } = new Collection<OperatingHour>();
    
    public ICollection<StockItem> StockItems { get; private set; } = new Collection<StockItem>();

    public ICollection<StockItemBooking> StockItemBookings { get; private set; } = new Collection<StockItemBooking>();

    public Ulid? TenantId { get; private set; }
    
    public Ulid? UserId { get; private set; }
}