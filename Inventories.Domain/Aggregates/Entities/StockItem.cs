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

        public void UpdateUnitValue(UnitValue<decimal> quantity)
        {
            Quantity = quantity;
        }
        public void UpdateMetaData(JsonElement? metaData)
        {
            MetaData = metaData;
        }
        public void AddStockItemBooking(StockItemBooking stockItemBooking)
        {
            StockItemBookings.Add(stockItemBooking);
        }
        public void RemoveStockItemBooking(StockItemBooking stockItemBooking)
        {
            StockItemBookings.Remove(stockItemBooking);
        }
        
        public Product Product { get; private set; }
        public Ulid ProductId { get; private set; }
        
        public Depot Depot { get; private set; }
        public Ulid DepotId { get; private set; }
        
        /// <summary>
        /// Get the Available Quantity of products in StockItem.
        /// </summary>
        public UnitValue<decimal> Quantity { get; private set; }
        
        /// <summary>
        /// Get the StockItem MetaData.
        /// </summary>
        public JsonElement? MetaData  { get; private set; }
        
        public ICollection<StockItemBooking> StockItemBookings { get; private set; } = new Collection<StockItemBooking>();
        public Ulid? TenantId { get; set; }
        public Ulid? UserId { get; set; }
    }
}