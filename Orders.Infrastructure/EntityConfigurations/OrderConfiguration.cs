using BuildingBlocks.Utilities.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Aggregates;
using Orders.Domain.Aggregates.Entities.Enums;

namespace Orders.Infrastructure.EntityConfigurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.RowVersion)
                   .HasColumnName("RowVersion")
                   .IsRowVersion()
                   .IsConcurrencyToken();
            
            builder.Property(e => e.Status)
                   .HasColumnName("Status")
                   .HasConversion<EnumerationConverter<OrderStatus>>()
                   .IsRequired();
            
            builder.Property(e => e.Type).HasColumnName("Type").HasMaxLength(255);
            builder.HasIndex(e => e.Type);
            
            builder.OwnsOne(o => o.BillingAddress, y =>
            {
                y.ToTable("OrderBillingAddress");
                y.Property(e => e.Street).HasMaxLength(255).IsRequired();
                y.Property(e => e.Number).HasMaxLength(255);
                y.Property(e => e.ZipCode).HasMaxLength(255).IsRequired();
                y.Property(e => e.City).HasMaxLength(255).IsRequired();
                y.Property(e => e.Country).HasMaxLength(255).IsRequired();
                y.Property(e => e.State).HasMaxLength(255);
                y.Property(e => e.Point).HasColumnType("geography");
                y.Ignore(e => e.Geography);
            });
            
            builder.OwnsOne(o => o.ShippingAddress, y => 
            {
                y.ToTable("OrderShippingAddress");
                y.Property(e => e.Street).HasMaxLength(255).IsRequired();
                y.Property(e => e.Number).HasMaxLength(255);
                y.Property(e => e.ZipCode).HasMaxLength(255).IsRequired();
                y.Property(e => e.City).HasMaxLength(255).IsRequired();
                y.Property(e => e.Country).HasMaxLength(255).IsRequired();
                y.Property(e => e.State).HasMaxLength(255);
                y.Property(e => e.Point).HasColumnType("geography");
                y.Ignore(e => e.Geography);
            });
            
            builder.OwnsOne(o => o.TransportAddress, y => 
            {
                y.ToTable("OrderTransportAddress");
                y.Property(e => e.Street).HasMaxLength(255).IsRequired();
                y.Property(e => e.Number).HasMaxLength(255);
                y.Property(e => e.ZipCode).HasMaxLength(255).IsRequired();
                y.Property(e => e.City).HasMaxLength(255).IsRequired();
                y.Property(e => e.Country).HasMaxLength(255).IsRequired();
                y.Property(e => e.State).HasMaxLength(255);
                y.Property(e => e.Point).HasColumnType("geography");
                y.Ignore(e => e.Geography);
            });

            builder.OwnsMany(o => o.TimeFrames, y =>
            {
                y.Property(p => p.From).IsRequired();
                y.Property(p => p.To).IsRequired();
                y.Property(p => p.DayOfWeek).IsRequired();
            });
            
            builder.Property(u => u.UserId).IsRequired();
            builder.HasIndex(u => u.UserId);
            
            builder.Property(u => u.TenantId).IsRequired();
            builder.HasIndex(u => u.TenantId);

            builder.OwnsMany(o => o.Depots, y =>
            {
                y.Property(e => e.DepotId).IsRequired();
                y.HasIndex(e => e.DepotId);
            });

            builder.Property(e => e.Description).HasColumnName("Description").HasColumnType("text");
            
            builder.Property(u => u.CreatedBy);
            builder.Property(u => u.CreatedDateUtcAt);
            builder.Property(u => u.CreatedTimeUtcAt);
            
            builder.Property(u => u.UpdatedBy);
            builder.Property(u => u.UpdatedDateUtcAt);
            builder.Property(u => u.UpdatedTimeUtcAt);

            builder.Property(u => u.DeletedBy);
            builder.Property(u => u.DeletedDateUtcAt);
            builder.Property(u => u.DeletedTimeUtcAt);
            
            builder.Property(u => u.IsDeleted);
        }
    }
}