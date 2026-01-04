namespace Orders.Domain.Entities;

public enum OrderStatus
{
    Created,
    Booked,
    Confirmed,
    Cancelled,
    Expired,
    Completed
}
