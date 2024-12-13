namespace Orders.Domain.Aggregates.Dtos;

public record OrderAddressDto
{
    private OrderAddressDto(AddressDto? billingAddress, AddressDto? shippingAddress, AddressDto? transportAddress)
    {
        BillingAddress = billingAddress;
        ShippingAddress = shippingAddress;
        TransportAddress = transportAddress;
    }
    public static OrderAddressDto Create(AddressDto? billingAddress, AddressDto? shippingAddress, AddressDto? transportAddress)
    {
        return new OrderAddressDto(billingAddress, shippingAddress, transportAddress);
    }

    /// <summary>
    /// Gets or sets the billing address of the order.
    /// </summary>
    public AddressDto? BillingAddress { get; init; }

    /// <summary>
    /// Gets or sets the shipping address of the order.
    /// </summary>
    public AddressDto? ShippingAddress { get; init; }

    /// <summary>
    /// Gets or sets the transport address of the order.
    /// </summary>
    public AddressDto? TransportAddress { get; init; }
}