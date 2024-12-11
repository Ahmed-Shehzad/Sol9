using System.Collections.ObjectModel;
using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Inventories.Domain.Aggregates.Entities
{
    public class StockItem : BaseEntity, ITenantDependent, IUserDependent
    {
        private StockItem(UnitValue<decimal> quantity, Product product, Depot depot, JsonElement? metaData) : base(Ulid.NewUlid())
        {
            Quantity = quantity;
            MetaData = metaData;
            
            DepotId = depot.Id;
            Depot = depot;
            
            ProductId = product.Id;
            Product = product;
        }

        public static StockItem Create(UnitValue<decimal> quantity, Product product, Depot depot, JsonElement? metaData)
        {
            return new StockItem(quantity, product, depot, metaData);
        }

        /// <summary>
        /// Updates the unit value of the stock item.
        /// </summary>
        /// <param name="quantity">The new unit value of the stock item.</param>
        /// <remarks>
        /// This method updates the Quantity property of the StockItem with the provided unit value.
        /// </remarks>
        public void UpdateUnitValue(UnitValue<decimal> quantity)
        {
            Quantity = quantity;
        }
        
        /// <summary>
        /// Updates the metadata of the stock item.
        /// </summary>
        /// <param name="metaData">The new metadata of the stock item. This can be null to remove existing metadata.</param>
        /// <remarks>
        /// This method updates the MetaData property of the StockItem with the provided JSON element.
        /// If the provided JSON element is null, it removes any existing metadata.
        /// </remarks>
        public void UpdateMetaData(JsonElement? metaData)
        {
            MetaData = metaData;
        }
        
        /// <summary>
        /// Adds a new stock item booking to the collection of bookings associated with this stock item.
        /// </summary>
        /// <param name="stockItemBooking">The stock item booking to be added. This cannot be null.</param>
        /// <remarks>
        /// This method adds the provided stock item booking to the StockItemBookings collection.
        /// If the provided stock item booking is null, an ArgumentNullException will be thrown.
        /// </remarks>
        public void AddStockItemBooking(StockItemBooking stockItemBooking)
        {
            StockItemBookings.Add(stockItemBooking);
        }
        
        /// <summary>
        /// Removes a stock item booking from the collection of bookings associated with this stock item.
        /// </summary>
        /// <param name="stockItemBooking">The stock item booking to be removed. This cannot be null.</param>
        /// <remarks>
        /// This method removes the provided stock item booking from the StockItemBookings collection.
        /// If the provided stock item booking is null, an ArgumentNullException will be thrown.
        /// </remarks>
        public void RemoveStockItemBooking(StockItemBooking stockItemBooking)
        {
            StockItemBookings.Remove(stockItemBooking);
        }
        
        /// <summary>
        /// Gets the product associated with this stock item.
        /// </summary>
        /// <value>
        /// The product object. This property is read-only and cannot be modified directly.
        /// </value>
        public Product Product { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the product associated with this stock item.
        /// </summary>
        /// <value>
        /// The unique identifier of the product. This property is read-only and cannot be modified directly.
        /// </value>
        public Ulid ProductId { get; private set; }

        /// <summary>
        /// Gets the depot associated with this stock item.
        /// </summary>
        /// <value>
        /// The depot object. This property is read-only and cannot be modified directly.
        /// </value>
        public Depot Depot { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the depot associated with this stock item.
        /// </summary>
        /// <value>
        /// The unique identifier of the depot. This property is read-only and cannot be modified directly.
        /// </value>
        public Ulid DepotId { get; private set; }
        
        /// <summary>
        /// Get the Available Quantity of products in StockItem.
        /// </summary>
        public UnitValue<decimal> Quantity { get; private set; }
        
        /// <summary>
        /// Get the StockItem MetaData.
        /// </summary>
        public JsonElement? MetaData  { get; private set; }
        
        /// <summary>
        /// Gets the collection of stock item bookings associated with this stock item.
        /// </summary>
        /// <value>
        /// A collection of StockItemBooking objects. This property is read-only and cannot be modified directly.
        /// The collection is initialized as an empty collection in the constructor.
        /// </value>
        public ICollection<StockItemBooking> StockItemBookings { get; private set; } = new Collection<StockItemBooking>();

        /// <summary>
        /// Gets the unique identifier of the tenant associated with this stock item.
        /// </summary>
        /// <value>
        /// The unique identifier of the tenant. This property is read-only and cannot be modified directly.
        /// The value is nullable, meaning it can be null if the stock item is not associated with a specific tenant.
        /// </value>
        public Ulid? TenantId { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the user associated with this stock item.
        /// </summary>
        /// <value>
        /// The unique identifier of the user. This property is read-only and cannot be modified directly.
        /// The value is nullable, meaning it can be null if the stock item is not associated with a specific user.
        /// </value>
        public Ulid? UserId { get; private set; }
    }
}