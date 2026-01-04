using Sol9.Core;

namespace Bookings.Domain.Entities;

public enum BookingStatus
{
    Created,
    Confirmed,
    Cancelled
}

public class Booking : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public BookingStatus Status { get; private set; }

    private Booking()
    {
    }

    private Booking(Guid orderId, string customerName, BookingStatus status)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Status = status;
    }

    public static Booking Create(Guid orderId, string customerName) => new(orderId, customerName, BookingStatus.Created);

    public void MarkConfirmed() => Status = BookingStatus.Confirmed;
    public void MarkCancelled() => Status = BookingStatus.Cancelled;
}
