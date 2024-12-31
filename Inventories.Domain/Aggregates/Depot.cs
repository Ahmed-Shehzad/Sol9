using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using BuildingBlocks.Extensions.Types;
using Inventories.Domain.Aggregates.Entities;
using Inventories.Domain.Aggregates.Entities.ValueObjects;

namespace Inventories.Domain.Aggregates;

/// <summary>
/// Represents a physical location where stock items are stored and managed.
/// </summary>
public class Depot : AggregateRoot, ITenantDependent, IUserDependent
{
    private Depot(string name, Address address, Contact  contact)
    {
        Name = name;
        Address = address;
        Contact = contact;
    }
    public static Depot Create(string name, Address address, Contact contact)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Depot(name, address, contact);
    }
    
    /// <summary>
    /// Updates the depot's name, address, and contact information.
    /// </summary>
    /// <param name="name">The new name of the depot.</param>
    /// <param name="address">The new physical address of the depot.</param>
    /// <param name="contact">The new contact information for the depot.</param>
    public void Update(string name, Address address, Contact contact)
    {
        Name = name;
        Address = address;
        Contact = contact;
    }
    
    /// <summary>
    /// Adds a single operating hour to the depot's collection of operating hours.
    /// </summary>
    /// <param name="operatingHour">The operating hour to be added.</param>
    public void AddOperatingHour(OperatingHour operatingHour)
    {
        OperatingHours.Add(operatingHour);
    }
    
    /// <summary>
    /// Adds a collection of operating hours to the depot's collection of operating hours.
    /// </summary>
    /// <param name="operatingHours">The collection of operating hours to be added.</param>
    public void AddOperatingHours(ICollection<OperatingHour> operatingHours)
    {
        OperatingHours.AddRange(operatingHours);
    }
    
    /// <summary>
    /// Removes a single operating hour from the depot's collection of operating hours.
    /// </summary>
    /// <param name="operatingHour">The operating hour to be removed.</param>
    public void RemoveOperatingHour(OperatingHour operatingHour)
    {
        OperatingHours.Remove(operatingHour);
    }
    
    /// <summary>
    /// Removes a collection of operating hours from the depot's collection of operating hours.
    /// </summary>
    /// <param name="operatingHours">The collection of operating hours to be removed.</param>
    public void RemoveOperatingHours(ICollection<OperatingHour> operatingHours)
    {
        OperatingHours.RemoveRange(operatingHours);
    }
    
    /// <summary>
    /// Adds a single stock item to the depot's collection of stock items.
    /// </summary>
    /// <param name="stockItem">The stock item to be added.</param>
    public void AddStockItem(StockItem stockItem)
    {
        StockItems.Add(stockItem);
    }
    
    /// <summary>
    /// Adds a collection of stock items to the depot's collection of stock items.
    /// </summary>
    /// <param name="stockItems">The collection of stock items to be added.</param>
    public void AddStockItems(ICollection<StockItem> stockItems)
    {
        StockItems.AddRange(stockItems);
    }
    
    /// <summary>
    /// Removes a single stock item from the depot's collection of stock items.
    /// </summary>
    /// <param name="stockItem">The stock item to be removed.</param>
    public void RemoveStockItem(StockItem stockItem)
    {
        StockItems.Remove(stockItem);
    }
    
    /// <summary>
    /// Removes a collection of stock items from the depot's collection of stock items.
    /// </summary>
    /// <param name="stockItems">The collection of stock items to be removed.</param>
    public void RemoveStockItems(ICollection<StockItem> stockItems)
    {
        StockItems.RemoveRange(stockItems);
    }
    
    /// <summary>
    /// Adds a single stock item booking to the depot's collection of stock item bookings.
    /// </summary>
    /// <param name="stockItemBooking">The stock item booking to be added.</param>
    public void AddStockItemBooking(StockItemBooking stockItemBooking)
    {
        StockItemBookings.Add(stockItemBooking);
    }
    
    /// <summary>
    /// Adds a collection of stock item bookings to the depot's collection of stock item bookings.
    /// </summary>
    /// <param name="stockItemBookings">The collection of stock item bookings to be added.</param>
    public void AddStockItemBookings(ICollection<StockItemBooking> stockItemBookings)
    {
        StockItemBookings.AddRange(stockItemBookings);
    }
    
    /// <summary>
    /// Removes a single stock item booking from the depot's collection of stock item bookings.
    /// </summary>
    /// <param name="stockItemBooking">The stock item booking to be removed.</param>
    public void RemoveStockItemBooking(StockItemBooking stockItemBooking)
    {
        StockItemBookings.Remove(stockItemBooking);
    }
    
    /// <summary>
    /// Removes a collection of stock item bookings from the depot's collection of stock item bookings.
    /// </summary>
    /// <param name="stockItemBookings">The collection of stock item bookings to be removed.</param>
    public void RemoveStockItemBookings(ICollection<StockItemBooking> stockItemBookings)
    {
        StockItemBookings.RemoveRange(stockItemBookings);
    }

    /// <summary>
    /// Gets or sets the name of the depot.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the physical address of the depot.
    /// </summary>
    public Address Address { get; private set; }

    /// <summary>
    /// Gets or sets the contact information for the depot.
    /// </summary>
    public Contact Contact { get; private set; }
    
    /// <summary>
    /// Gets a collection of operating hours for the depot.
    /// </summary>
    public ICollection<OperatingHour> OperatingHours { get; private set; } = new List<OperatingHour>();
    
    /// <summary>
    /// Gets a collection of stock items available in the depot.
    /// </summary>
    public ICollection<StockItem> StockItems { get; private set; } = new List<StockItem>();

    /// <summary>
    /// Gets a collection of stock item bookings made for the depot.
    /// </summary>
    public ICollection<StockItemBooking> StockItemBookings { get; private set; } = new List<StockItemBooking>();

    /// <summary>
    /// Gets or sets the unique identifier of the tenant associated with the depot.
    /// </summary>
    public Ulid? TenantId { get; private set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the depot.
    /// </summary>
    public Ulid? UserId { get; private set; }
}