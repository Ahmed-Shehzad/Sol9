using Humanizer;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Orders.Domain.Entities;

namespace Orders.Infrastructure.Configurations;

public class OrdersConfigurations : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        _ = builder.ToTable(nameof(Order).Pluralize());

        _ = builder.HasKey(o => o.Id);

        _ = builder.Property(o => o.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        _ = builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        _ = builder.Property(o => o.Status)
            .HasMaxLength(100)
            .IsRequired();

        _ = builder.Property(o => o.CreatedAtUtc);
        _ = builder.Property(o => o.UpdatedAtUtc);
        _ = builder.Property(o => o.DeletedAtUtc);
        _ = builder.Property(o => o.IsDeleted);
    }
}
