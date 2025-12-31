using Sol9.Core;

namespace Bookings.Domain.Entities;

public class Booking : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;

    private Booking()
    {
    }

    private Booking(Guid orderId, string customerName, string status)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Status = status;
    }

    public static Booking Create(Guid orderId, string customerName)
        => new(orderId, customerName, "Created");

    public void MarkConfirmed()
    {
        Status = "Confirmed";
        ApplyUpdateDateTime();
    }
}
