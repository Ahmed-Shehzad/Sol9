using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates;
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
    
    public void RemoveOperatingHour(OperatingHour operatingHour)
    {
        OperatingHours.Remove(operatingHour);
    }
    
    public void AddStockItem(StockItem stockItem)
    {
        StockItems.Add(stockItem);
    }
    
    public void RemoveStockItem(StockItem stockItem)
    {
        StockItems.Remove(stockItem);
    }
    public void AddStockItemBooking(StockItemBooking stockItemBooking)
    {
        StockItemBookings.Add(stockItemBooking);
    }
    public void RemoveStockItemBooking(StockItemBooking stockItemBooking)
    {
        StockItemBookings.Remove(stockItemBooking);
    }

    public string Name { get; private set; }

    public Address Address { get; private set; }

    public ICollection<OperatingHour> OperatingHours { get; private set; }
    
    public ICollection<StockItem> StockItems { get; private set; }
    
    public ICollection<StockItemBooking> StockItemBookings { get; private set; }
    public Ulid? TenantId { get; set; }
    public Ulid? UserId { get; set; }
}