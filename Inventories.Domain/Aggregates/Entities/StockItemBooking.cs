using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Inventories.Domain.Aggregates.Entities
{
    public class StockItemBooking : BaseEntity, ITenantDependent, IUserDependent
    {
        private StockItemBooking(
            UnitValue<decimal> quantity,
            StockItem stockItem,
            Product product,
            Depot depot,
            Ulid tripId,
            Ulid stopItemId) : base(Ulid.NewUlid())
        {
            Quantity = quantity;
            
            DepotId = depot.Id;
            Depot = depot;
            
            StockItemId = stockItem.Id;
            StockItem = stockItem;
            
            ProductId = product.Id;
            Product = product;
            
            TripId = tripId;
            StopItemId = stopItemId;
        }
        
        public static StockItemBooking Create(UnitValue<decimal> quantity, StockItem stockItem, Product product, Depot depot, Ulid tripId, Ulid stopItemId)
        {
            return new StockItemBooking(quantity, stockItem, product, depot, tripId, stopItemId);
        }

        public Product Product { get; private set; }
        public Ulid ProductId { get; private set; }
        public Depot Depot { get; private set; }
        public Ulid DepotId { get; private set; }
        
        public StockItem StockItem { get; private set; }
        public Ulid StockItemId { get; private set; }

        public Ulid TripId { get; private set; }
        public Ulid StopItemId { get; private set; }
        
        public UnitValue<decimal> Quantity { get; private set; }
        public Ulid? TenantId { get; set; }
        public Ulid? UserId { get; set; }
    }
}