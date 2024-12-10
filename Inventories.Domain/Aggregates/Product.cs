using System.Collections.ObjectModel;
using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using BuildingBlocks.Extensions.Types;
using Inventories.Domain.Aggregates.Entities;
using Inventories.Domain.Aggregates.Entities.Enums;

namespace Inventories.Domain.Aggregates
{
    public class Product : AggregateRoot, ITenantDependent, IUserDependent
    {
        private Product(string name, 
            string description,
            string manufacturer, 
            ProductStatus status, 
            UnitValue<decimal> unitPrice, 
            Ulid userId,
            Ulid tenantId)
        {
            Name = name;
            Description = description;
            Manufacturer = manufacturer;
            Status = status;
            UnitPrice = unitPrice;
            UserId = userId;
            TenantId = tenantId;
        }
        public static Product Create(string name, string description, string manufacturer, ProductStatus status, UnitValue<decimal> unitPrice, Ulid userId, Ulid tenantId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return new Product(name, description, manufacturer, status, unitPrice, userId, tenantId);
        }

        /// <summary>
        /// Updates the product's details.
        /// </summary>
        /// <param name="name">The new name of the product.</param>
        /// <param name="description">The new description of the product.</param>
        /// <param name="manufacturer">The new manufacturer of the product.</param>
        public void Update(string name, string description, string manufacturer)
        {
            Name = name;
            Description = description;
            Manufacturer = manufacturer;
        }

        /// <summary>
        /// Updates the product's status.
        /// </summary>
        /// <param name="status">The new status of the product. This cannot be null.</param>
        /// <remarks>
        /// This method updates the product's status to the specified value.
        /// </remarks>
        public void UpdateProductStatus(ProductStatus status)
        {
            ArgumentNullException.ThrowIfNull(status);
            Status = status;
        }
        
        /// <summary>
        /// Updates the unit price of the product.
        /// </summary>
        /// <param name="unitPrice">The new unit price of the product. This cannot be null.</param>
        /// <remarks>
        /// This method updates the unit price of the product to the specified value.
        /// </remarks>
        public void UpdateUnitPrice(UnitValue<decimal> unitPrice)
        {
            ArgumentNullException.ThrowIfNull(unitPrice);
            UnitPrice = unitPrice;
        }
        
        /// <summary>
        /// Adds a new stock item to the product's collection of stock items.
        /// </summary>
        /// <param name="stockItem">The stock item to be added. This cannot be null.</param>
        public void AddStockItem(StockItem stockItem)
        {
            ArgumentNullException.ThrowIfNull(stockItem);
            StockItems.Add(stockItem);
        }
        /// <summary>
        /// Adds a collection of stock items to the product's collection of stock items.
        /// </summary>
        /// <param name="stockItems">The collection of stock items to be added. This cannot be null.</param>
        public void AddStockItems(ICollection<StockItem> stockItems)
        {
            ArgumentNullException.ThrowIfNull(stockItems);
            StockItems.AddRange(stockItems);
        }
        /// <summary>
        /// Adds a new stock item booking to the product's collection of stock item bookings.
        /// </summary>
        /// <param name="stockItemBooking">The stock item booking to be added. This cannot be null.</param>
        public void AddStockItemBooking(StockItemBooking stockItemBooking)
        {
            ArgumentNullException.ThrowIfNull(stockItemBooking);
            StockItemBookings.Add(stockItemBooking);
        }
        /// <summary>
        /// Adds a collection of stock item bookings to the product's collection of stock item bookings.
        /// </summary>
        /// <param name="stockItemBookings">The collection of stock item bookings to be added. This cannot be null.</param>
        public void AddStockItemBookings(ICollection<StockItemBooking> stockItemBookings)
        {
            ArgumentNullException.ThrowIfNull(stockItemBookings);
            StockItemBookings.AddRange(stockItemBookings);
        }
        /// <summary>
        /// Removes a specific stock item from the product's collection of stock items.
        /// </summary>
        /// <param name="stockItem">The stock item to be removed. This cannot be null.</param>
        /// <remarks>
        /// This method will remove the specified stock item from the product's collection of stock items.
        /// If the stock item is not found in the collection, no action will be taken.
        /// </remarks>
        public void RemoveStockItem(StockItem stockItem)
        {
            ArgumentNullException.ThrowIfNull(stockItem);
            StockItems.Remove(stockItem);
        }
        /// <summary>
        /// Removes a collection of specific stock items from the product's collection of stock items.
        /// </summary>
        /// <param name="stockItems">The collection of stock items to be removed. This cannot be null.</param>
        /// <remarks>
        /// This method will remove the specified stock items from the product's collection of stock items.
        /// If any of the stock items are not found in the collection, no action will be taken for those specific items.
        /// </remarks>
        public void RemoveStockItems(ICollection<StockItem> stockItems)
        {
            ArgumentNullException.ThrowIfNull(stockItems);
            StockItems.RemoveRange(stockItems);
        }
        /// <summary>
        /// Removes a specific stock item booking from the product's collection of stock item bookings.
        /// </summary>
        /// <param name="stockItemBooking">The stock item booking to be removed. This cannot be null.</param>
        /// <remarks>
        /// This method will remove the specified stock item booking from the product's collection of stock item bookings.
        /// If the stock item booking is not found in the collection, no action will be taken.
        /// </remarks>
        public void RemoveStockItemBooking(StockItemBooking stockItemBooking)
        {
            ArgumentNullException.ThrowIfNull(stockItemBooking);
            StockItemBookings.Remove(stockItemBooking);
        }
        /// <summary>
        /// Removes a collection of specific stock item bookings from the product's collection of stock item bookings.
        /// </summary>
        /// <param name="stockItemBookings">The collection of stock item bookings to be removed. This cannot be null.</param>
        /// <remarks>
        /// This method will remove the specified stock item bookings from the product's collection of stock item bookings.
        /// If any of the stock item bookings are not found in the collection, no action will be taken for those specific items.
        /// </remarks>
        public void RemoveStockItemBookings(ICollection<StockItemBooking> stockItemBookings)
        {
            ArgumentNullException.ThrowIfNull(stockItemBookings);
            StockItemBookings.RemoveRange(stockItemBookings);
        }

        /// <summary>
        /// Get the name of the Product.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Get the Description of the Product.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Get the Manufacturer of the Product.
        /// </summary>
        public string Manufacturer { get; private set; }

        /// <summary>
        /// Get the Product Status, whether it is Active or Expired.
        /// </summary>
        public ProductStatus Status { get; private set; }
        
        /// <summary>
        /// Get the Unit Price of the Product.
        /// </summary>
        public UnitValue<decimal> UnitPrice { get; private set; }
        
        /// <summary>
        /// Gets the collection of stock items associated with the product.
        /// </summary>
        /// <remarks>
        /// This collection is initialized as an empty collection and is intended to hold all stock items related to the product.
        /// </remarks>
        public ICollection<StockItem> StockItems { get; private set; } = new Collection<StockItem>();
        
        /// <summary>
        /// Gets the collection of stock item bookings associated with the product.
        /// </summary>
        /// <remarks>
        /// This collection is initialized as an empty collection and is intended to hold all stock item bookings related to the product.
        /// </remarks>
        public ICollection<StockItemBooking> StockItemBookings { get; private set; } = new Collection<StockItemBooking>();
        
        /// <summary>
        /// Gets the metadata associated with the product.
        /// </summary>
        /// <remarks>
        /// This property is intended to hold any additional information or metadata related to the product.
        /// The metadata is represented as a JSON element and can be null if no metadata is provided.
        /// </remarks>
        public JsonElement? MetaData  { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the user who owns the product.
        /// </summary>
        /// <remarks>
        /// This property is intended to track the user who created or modified the product.
        /// The user identifier is represented as a Ulid (Universally Unique Lexicographically Sortable Identifier).
        /// </remarks>
        public Ulid? UserId { get; private set; }
        
        /// <summary>
        /// Gets the unique identifier of the tenant to which the product belongs.
        /// </summary>
        /// <remarks>
        /// This property is intended to track the tenant to which the product belongs.
        /// The tenant identifier is represented as a Ulid (Universally Unique Lexicographically Sortable Identifier).
        /// </remarks>
        public Ulid? TenantId { get; private set; }
    }
}