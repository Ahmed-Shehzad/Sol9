using System.Collections.ObjectModel;
using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
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
            return new Product(name, description, manufacturer, status, unitPrice, userId, tenantId);
        }
        
        public void UpdateName(string name)
        {
            Name = name;
        }
        public void UpdateDescription(string description)
        {
            Description = description;
        }
        public void UpdateManufacturer(string manufacturer)
        {
            Manufacturer = manufacturer;
        }
        public void UpdateStatus(ProductStatus status)
        {
            Status = status;
        }
        public void UpdateUnitPrice(UnitValue<decimal> unitPrice)
        {
            UnitPrice = unitPrice;
        }
        public void UpdateMetaData(JsonElement? metaData)
        {
            MetaData = metaData;
        }
        public void AddStockItem(StockItem stockItem)
        {
            StockItems.Add(stockItem);
        }
        public void AddStockItemBooking(StockItemBooking stockItemBooking)
        {
            StockItemBookings.Add(stockItemBooking);
        }
        public void RemoveStockItem(StockItem stockItem)
        {
            StockItems.Remove(stockItem);
        }
        public void RemoveStockItemBooking(StockItemBooking stockItemBooking)
        {
            StockItemBookings.Remove(stockItemBooking);
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
        
        public ICollection<StockItem> StockItems { get; private set; } = new Collection<StockItem>();
        
        public ICollection<StockItemBooking> StockItemBookings { get; private set; } = new Collection<StockItemBooking>();
        
        public JsonElement? MetaData  { get; private set; }

        public Ulid? UserId { get; set; }
        public Ulid? TenantId { get; set; }
    }
}