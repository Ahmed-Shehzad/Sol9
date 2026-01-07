using Orders.Domain.Events.Integration;

using Sol9.Core;

namespace Orders.Domain.Entities;

public class Order : AggregateRoot
{
    public string CustomerName { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    private Order()
    {
    }

    private Order(string customerName, decimal totalAmount)
    {
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Status = OrderStatus.Created;

        ApplyEvent(new OrderCreatedIntegrationEvent(Id));
    }

    public static Order Create(string customerName, decimal totalAmount) => new(customerName, totalAmount);

    public void MarkBooked() => Status = OrderStatus.Booked;
    public void MarkConfirmed() => Status = OrderStatus.Confirmed;
    public void MarkCancelled()
    {
        Status = OrderStatus.Cancelled;
        ApplyEvent(new OrderCancelledIntegrationEvent(Id, CustomerName));
    }
    public void MarkCompleted() => Status = OrderStatus.Completed;
    public void MarkExpired() => Status = OrderStatus.Expired;
}
