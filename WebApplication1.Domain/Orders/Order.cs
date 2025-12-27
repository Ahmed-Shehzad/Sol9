namespace WebApplication1.Domain.Orders;

public sealed class Order
{
    private Order(Guid id, string customerName, decimal total, DateTimeOffset createdAt)
    {
        Id = id;
        CustomerName = customerName;
        Total = total;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public string CustomerName { get; }

    public decimal Total { get; }

    public DateTimeOffset CreatedAt { get; }

    public static Order Create(string customerName, decimal total, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        if (total <= 0)
            throw new ArgumentOutOfRangeException(nameof(total), "Total must be greater than zero.");

        return new Order(Guid.NewGuid(), customerName, total, createdAt);
    }
}
