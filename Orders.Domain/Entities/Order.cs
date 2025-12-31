using Sol9.Core;

namespace Orders.Domain.Entities;

public class Order : AggregateRoot
{
    public string CustomerName { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; } = string.Empty;

    private Order()
    {
    }

    private Order(string customerName, decimal totalAmount)
    {
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Status = "Created";
    }

    public static Order Create(string customerName, decimal totalAmount)
        => new(customerName, totalAmount);

    public void MarkBooked()
    {
        Status = "Booked";
        ApplyUpdateDateTime();
    }
}
