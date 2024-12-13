using BuildingBlocks.Utilities.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Aggregates.Entities;

namespace Orders.Infrastructure.EntityConfigurations
{
    public class OrderItemConfiguration: IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder
                .HasOne(e => e.Order)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.OrderId);

            builder.Property(e => e.ProductId);
            builder.HasIndex(e => e.ProductId);

            builder.Property(e => e.StopItemId);
            builder.HasIndex(e => e.StopItemId);

            builder.Property(e => e.TripId);
            builder.HasIndex(e => e.TripId);
            
            builder.OwnsOne(e => e.OrderItemInfo, y =>
            {
                y.ToTable("OrderItemInfo");
                y.Property(q => q.Description);
                y.Property(q => q.MetaData).HasConversion<JsonElementConverter>();
                
                y.OwnsOne(q => q.Quantity, navigationBuilder =>
                {
                    navigationBuilder.ToTable("OrderItemQuantity");
                    navigationBuilder.Property(e => e.Unit).HasMaxLength(255).IsRequired();
                    navigationBuilder.Property(e => e.Value).HasColumnType("decimal(18, 3)").IsRequired();
                });
                
                y.OwnsOne(q => q.Weight, navigationBuilder =>
                {
                    navigationBuilder.ToTable("OrderItemWeight");
                    navigationBuilder.Property(e => e.Unit).HasMaxLength(255).IsRequired();
                    navigationBuilder.Property(e => e.Value).HasColumnType("decimal(18, 3)").IsRequired();
                });
            });
        }
    }
}