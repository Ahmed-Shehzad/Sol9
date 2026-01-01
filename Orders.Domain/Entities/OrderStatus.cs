namespace Orders.Domain.Entities;

public enum OrderStatus
{
    Created,
    Booked,
    Cancelled,
    Expired,
    Completed
}
