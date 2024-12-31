using BuildingBlocks.Utilities.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Aggregates.Entities;

namespace Orders.Infrastructure.EntityConfigurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("gen_random_bytes(8)")
            .ValueGeneratedOnAddOrUpdate();
        
        builder
            .HasOne(e => e.Order)
            .WithMany(e => e.Items)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.ProductId);
        builder.HasIndex(e => e.ProductId);

        builder.Property(e => e.StopItemId);
        builder.HasIndex(e => e.StopItemId);

        builder.Property(e => e.TripId);
        builder.HasIndex(e => e.TripId);

        builder.ComplexProperty(e => e.OrderItemInfo, y =>
        {
            y.Property(q => q.Description).HasColumnName("Description".ToLowerInvariant());
            y.Property(q => q.MetaData).HasColumnName("MetaData".ToLowerInvariant()).HasConversion<JsonElementConverter>();

            y.ComplexProperty(q => q.Quantity, navigationBuilder =>
            {
                navigationBuilder.Property(e => e.Unit).HasColumnName("Quantity_Unit".ToLowerInvariant()).HasMaxLength(255).IsRequired();
                navigationBuilder.Property(e => e.Value).HasColumnName("Quantity_Value".ToLowerInvariant()).HasColumnType("decimal(18, 3)")
                    .IsRequired();
            });

            y.ComplexProperty(q => q.Weight, navigationBuilder =>
            {
                navigationBuilder.Property(e => e.Unit).HasColumnName("Weight_Unit".ToLowerInvariant()).HasMaxLength(255).IsRequired();
                navigationBuilder.Property(e => e.Value).HasColumnName("Weight_Value".ToLowerInvariant()).HasColumnType("decimal(18, 3)")
                    .IsRequired();
            });
        });
    }
}