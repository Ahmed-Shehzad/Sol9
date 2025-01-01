using BuildingBlocks.Domain.Aggregates.Dtos;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using Orders.Application.Dtos;
using Orders.Domain.Aggregates;
using Orders.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.Enums;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Application.Mappings;

public static class MapToDtos
{
    public static UnitValueDto<T> MapUnitValueToDto<T>(this UnitValue<T> unitValue)
    {
        return new UnitValueDto<T>(unitValue.Value, unitValue.Unit);
    }
    public static DocumentInfoDto MapOrderDocumentInfoToDto(this OrderDocumentInfo orderDocumentInfo)
    {
        return new DocumentInfoDto(orderDocumentInfo.Name, orderDocumentInfo.Type, orderDocumentInfo.Url);
    }

    public static OrderItemInfoDto MapOrderItemInfoToDto(this OrderItemInfo orderItemInfo)
    {
        return new OrderItemInfoDto(orderItemInfo.Quantity.MapUnitValueToDto(), 
                                    orderItemInfo.Description,
                                    orderItemInfo.Weight.MapUnitValueToDto(), 
                                    orderItemInfo.MetaData);
    }
    
    public static OrderItemDto MapOrderItemToDto(this OrderItem orderItem)
    {
        return new OrderItemDto(orderItem.OrderId, orderItem.ProductId, 
                                orderItem.StopItemId, orderItem.TripId, 
                                orderItem.OrderItemInfo.MapOrderItemInfoToDto(), 
                                orderItem.Id);
    }
    public static List<OrderItemDto> MapOrderItemsToDto(this List<OrderItem>? orderItems)
    {
        if (orderItems is null) return [];
        return orderItems.Select(orderItem => orderItem.MapOrderItemToDto()).ToList();
    }

    public static OrderDocumentDto MapOrderDocumentToDto(this OrderDocument orderDocument)
    {
        return new OrderDocumentDto(orderDocument.DocumentInfo.MapOrderDocumentInfoToDto(),
                                    orderDocument.OrderId,
                                    orderDocument.MetaData,
                                    orderDocument.TenantId,
                                    orderDocument.UserId,
                                    orderDocument.Id);
    }
    public static List<OrderDocumentDto> MapOrderDocumentsToDto(this List<OrderDocument>? orderDocuments)
    {
        if (orderDocuments is null) return [];
        return orderDocuments.Select(orderDocument => orderDocument.MapOrderDocumentToDto()).ToList();
    }

    public static AddressDto? MapAddressToDto(this Address? address)
    {
        if (address is null) return null;
        return new AddressDto(new GeographyDto(address.Geography?.Longitude, address.Geography?.Latitude),
                        address.Street,
                        address.Number,
                        address.ZipCode,
                        address.City,
                        address.State,
                        address.Country);
    }

    public static TimeFrameDto MapTimeFrameToDto(this OrderTimeFrame timeFrame)
    {
        return new TimeFrameDto(timeFrame.DayOfWeek, timeFrame.From, timeFrame.To);
    }

    public static List<TimeFrameDto> MapTimeFramesToDto(this List<OrderTimeFrame>? timeFrames)
    {
        if (timeFrames is null) return [];
        return timeFrames.Select(timeFrame => timeFrame.MapTimeFrameToDto()).ToList();
    }

    public static DepotDto MapDepotToDto(this Depot? depot)
    {
        return new DepotDto(depot?.DepotId);
    }
    
    public static List<DepotDto> MapDepotsToDto(this List<Depot>? depots)
    {
        if (depots is null) return [];
        return depots.Select(depot => depot.MapDepotToDto()).ToList();
    }

    public static OrderDto MapOrderToDto(this Order order)
    {
        return new OrderDto(order.Type, order.Description, 
            order.Status.ToString(), 
            order.BillingAddress?.MapAddressToDto(),
            order.ShippingAddress?.MapAddressToDto(),
            order.TransportAddress.MapAddressToDto(),
            order.TimeFrames.ToList().MapTimeFramesToDto(),
            order.Items.ToList().MapOrderItemsToDto(),
            order.Documents.ToList().MapOrderDocumentsToDto(),
            order.Depots.ToList().MapDepotsToDto(), 
            order.TenantId, order.UserId,
            order.Id);
    }

    public static List<OrderDto> MapOrdersToDto(this List<Order>? orders)
    {
        if (orders is null) return [];
        return orders.Select(order => order.MapOrderToDto()).ToList();
    }
}