using System.Text.Json;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities
{
    public class Document() : BaseEntity(Ulid.NewUlid()), ITenantDependent, IUserDependent
    {
        private Document(DocumentInfo documentDetails, Order order, JsonElement? metaData) : this()
        {
            DocumentDetails = documentDetails;
            OrderId = order.Id;
            Order = order;
            MetaData = metaData;
        }
        
        public static Document Create(DocumentInfo documentDetails, Order order, JsonElement? metaData)
        {
            return new Document(documentDetails, order, metaData);
        }

        public void Update(DocumentInfo documentDetails, Order order, JsonElement? metaData)
        {
            DocumentDetails = documentDetails;
            OrderId = order.Id;
            Order = order;
            MetaData = metaData;
        }
        
        public DocumentInfo DocumentDetails { get; private set; }

        public Ulid OrderId { get; private set; }

        public Order Order { get; private set; }

        public JsonElement? MetaData { get; private set; }

        public Ulid? TenantId { get; set; }
        
        public Ulid? UserId { get; set; }
    }
}