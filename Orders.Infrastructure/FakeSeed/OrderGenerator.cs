using Bogus;
using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.Utilities.Types;
using Orders.Domain.Aggregates;
using Orders.Domain.Aggregates.Entities;
using Orders.Domain.Aggregates.Entities.Enums;
using Orders.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Infrastructure.FakeSeed;

public static class OrderGenerator
{
    private static Faker<Address> AddressFaker
    {
        get => new Faker<Address>()
            .CustomInstantiator(f => Address.Create(
                geography: Coordinates.Create(
                    latitude: f.Address.Latitude(),
                    longitude: f.Address.Longitude()),
                street: f.Address.StreetAddress(),
                city: f.Address.City(),
                state: f.Address.State(),
                zipCode: f.Address.ZipCode(),
                country: f.Address.Country(),
                number: f.Address.BuildingNumber()
            ));
    }

    private static Faker<OrderItem> OrderItemFaker
    {
        get => new Faker<OrderItem>()
            .CustomInstantiator(f => OrderItem.Create(
                order: new Order(),
                productId: Ulid.NewUlid(),
                stopItemId: Ulid.NewUlid(),
                tripId: Ulid.NewUlid(),
                orderItemInfo: OrderItemInfo.Create(
                    quantity: UnitValue<decimal>.Create(f.Finance.Amount(), "kg"),
                    weight: UnitValue<decimal>.Create(f.Finance.Amount(), "kg"),
                    description: f.Lorem.Sentence(),
                    metaData: null
                )
            ));
    }

    private static Faker<OrderDocument> DocumentFaker
    {
        get => new Faker<OrderDocument>()
            .CustomInstantiator(f => OrderDocument.Create(
                documentInfo: OrderDocumentInfo.Create(
                    name: f.Company.CatchPhrase(),
                    type: f.Company.CompanyName(),
                    url: f.Internet.Url()
                ),
                order: new Order(),
                metaData: null
            ));
    }

    private static Faker<Depot> DepotFaker
    {
        get => new Faker<Depot>()
            .CustomInstantiator(_ => new Depot(Ulid.NewUlid()));
    }

    private static Faker<OrderTimeFrame> TimeFrameFaker
    {
        get => new Faker<OrderTimeFrame>()
            .CustomInstantiator(f =>
            {
                var from = TimeOnly.FromDateTime(f.Date.Recent());
                var to = TimeOnly.FromDateTime(f.Date.Soon());
                return OrderTimeFrame.Create(
                    dayOfWeek: f.Date.FutureDateOnly().DayOfWeek,
                    from: from,
                    to: to > from ? to : from.AddHours(1) // Ensure valid range
                );
            });
    }

    private static Faker<Order> OrderFaker
    {
        get => new Faker<Order>()
            .CustomInstantiator(f => Order.Create(
                type: f.Commerce.ProductMaterial(),
                description: f.Lorem.Sentence(),
                status: f.PickRandom(Enumeration.GetAll<OrderStatus>()),
                billingAddress: AddressFaker.Generate(),
                shippingAddress: AddressFaker.Generate(),
                transportAddress: AddressFaker.Generate()
            ))
            .RuleFor(o => o.Items, f => OrderItemFaker.Generate(f.Random.Int(1, 5)))
            .RuleFor(o => o.Documents, f => DocumentFaker.Generate(f.Random.Int(1, 3)));
    }

    private static readonly List<Order> Orders = [];

    public static List<Order> GenerateMinimumTenOrders(int count = 10)
    {
        count = count < 10 ? 10 : count;

        Orders.Clear();

        var generatedOrders = OrderFaker.Generate(count);

        foreach (var order in generatedOrders)
        {
            var tenantId = Ulid.NewUlid();
            var userId = Ulid.NewUlid();

            var documents = order.Documents
                .Select(orderDocument => OrderDocument.Create(orderDocument.DocumentInfo, order, orderDocument.MetaData))
                .ToList();

            documents = documents.Select(document =>
            {
                document.UpdateTenantId(tenantId);
                document.UpdateUserId(userId);
                return document;
            }).ToList();

            var orderItems = order.Items
                .Select(orderItem =>
                    OrderItem.Create(order, orderItem.ProductId, orderItem.StopItemId, orderItem.TripId, orderItem.OrderItemInfo))
                .ToList();

            var rebuiltOrder = Order.Create(
                type: order.Type,
                description: order.Description,
                status: order.Status,
                billingAddress: order.BillingAddress,
                shippingAddress: order.ShippingAddress,
                transportAddress: order.TransportAddress
            );

            rebuiltOrder.Items.AddRange(orderItems);
            rebuiltOrder.Documents.AddRange(documents);

            rebuiltOrder.UpdateTenantId(tenantId);
            rebuiltOrder.UpdateUserId(userId);

            rebuiltOrder.Depots.AddRange(DepotFaker.Generate(count));
            rebuiltOrder.TimeFrames.AddRange(TimeFrameFaker.Generate(count));
            
            Orders.Add(rebuiltOrder);
        }

        return Orders;
    }
}