using Orders.Domain.Aggregates.Dtos;
using Orders.Domain.Aggregates.Entities;

namespace Orders.Domain.Aggregates.Mappings;

public static class MapToDtos
{
    public static OrderItemDto MapOrderItemToDto(this OrderItem orderItem)
    {
        return OrderItemDto.Create(orderItem.Id, orderItem.OrderId, orderItem.Order, orderItem.ProductId, orderItem.StopItemId,
            orderItem.TripId, orderItem.OrderItemInfo);
    }
    public static List<OrderItemDto> MapOrderItemsToDto(this List<OrderItem> orderItems)
    {
        return orderItems.Select(orderItem => orderItem.MapOrderItemToDto()).ToList();
    }

    public static OrderDocumentDto MapOrderDocumentToDto(this OrderDocument orderDocument)
    {
        return OrderDocumentDto.Create(orderDocument.Id, orderDocument.DocumentInfo, orderDocument.OrderId, orderDocument.Order,
            orderDocument.MetaData, orderDocument.TenantId, orderDocument.UserId);
    }
    public static List<OrderDocumentDto> MapOrderDocumentsToDto(this List<OrderDocument> orderDocuments)
    {
        return orderDocuments.Select(orderDocument => orderDocument.MapOrderDocumentToDto()).ToList();
    }

    public static OrderDto MapOrderToDto(this Order order)
    {
        AddressDto? billingAddress = null;
        AddressDto? shippingAddress = null;
        AddressDto? transportAddress = null;

        if (order.BillingAddress is not null)
        {
            billingAddress = AddressDto.Create(order.BillingAddress.Geography, order.BillingAddress.Street, order.BillingAddress.Number,
                order.BillingAddress.ZipCode, order.BillingAddress.City, order.BillingAddress.State, order.BillingAddress.Country);
        }
        if (order.ShippingAddress is not null)
        {
            shippingAddress = AddressDto.Create(order.ShippingAddress.Geography, order.ShippingAddress.Street, order.ShippingAddress.Number,
                order.ShippingAddress.ZipCode, order.ShippingAddress.City, order.ShippingAddress.State, order.ShippingAddress.Country);
        }
        if (order.TransportAddress != null)
        {
            transportAddress = AddressDto.Create(order.TransportAddress.Geography, order.TransportAddress.Street,
                order.TransportAddress.Number,
                order.TransportAddress.ZipCode, order.TransportAddress.City, order.TransportAddress.State, order.TransportAddress.Country);
        }
        var orderAddress = OrderAddressDto.Create(billingAddress, shippingAddress, transportAddress);

        return OrderDto.Create(order.Id, order.Type, order.Description, order.Status, orderAddress, order.TimeFrames,
            order.Items.ToList().MapOrderItemsToDto(),
            order.Documents.ToList().MapOrderDocumentsToDto(),
            order.Depots, order.TenantId, order.UserId);
    }

    public static List<OrderDto> MapOrdersToDto(this List<Order> orders)
    {
        return orders.Select(order => order.MapOrderToDto()).ToList();
    }
}