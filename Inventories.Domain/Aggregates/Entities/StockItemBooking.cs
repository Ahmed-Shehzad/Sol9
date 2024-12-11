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

        /// <summary>
        /// The product associated with the stock item.
        /// </summary>
        public Product Product { get; private set; }
        
        /// <summary>
        /// The unique identifier of the product.
        /// </summary>
        public Ulid ProductId { get; private set; }
        
        /// <summary>
        /// The depot where the stock item is being booked.
        /// </summary>
        public Depot Depot { get; private set; }
        
        /// <summary>
        /// The unique identifier of the depot.
        /// </summary>
        public Ulid DepotId { get; private set; }
        
        /// <summary>
        /// The stock item to be booked.
        /// </summary>
        public StockItem StockItem { get; private set; }
        
        /// <summary>
        /// The unique identifier of the stock item.
        /// </summary>
        public Ulid StockItemId { get; private set; }

        /// <summary>
        /// The unique identifier of the trip.
        /// </summary>
        public Ulid TripId { get; private set; }
        
        /// <summary>
        /// The unique identifier of the trip stop.
        /// </summary>
        public Ulid StopItemId { get; private set; }
        
        /// <summary>
        /// The quantity of the stock item to be booked.
        /// </summary>
        public UnitValue<decimal> Quantity { get; private set; }
        
        /// <summary>
        /// The unique identifier of the tenant.
        /// </summary>
        public Ulid? TenantId { get; private set; }
        
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Ulid? UserId { get; private set; }
    }
}