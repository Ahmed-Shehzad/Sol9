using BuildingBlocks.Utilities.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Aggregates.Entities;

namespace Orders.Infrastructure.EntityConfigurations
{
    public class OrderDocumentConfiguration: IEntityTypeConfiguration<OrderDocument>
    {
        public void Configure(EntityTypeBuilder<OrderDocument> builder)
        {
            builder.HasKey(e => e.Id);

            builder.OwnsOne(o => o.DocumentInfo, navigationBuilder =>
            {
                navigationBuilder.Property(e => e.Name)
                                 .HasColumnName("Name")
                                 .HasMaxLength(255)
                                 .IsRequired();
                
                navigationBuilder.Property(e => e.Type)
                                 .HasColumnName("Type")
                                 .HasMaxLength(255)
                                 .IsRequired();
                
                navigationBuilder.Property(e => e.Url)
                                 .HasColumnName("Url")
                                 .HasColumnType("text")
                                 .IsRequired();
            });
            
            builder.Property(u => u.MetaData).HasConversion<JsonElementConverter>();
            
            builder.Property(u => u.UserId).IsRequired();
            builder.HasIndex(u => u.UserId);

            builder.Property(u => u.TenantId).IsRequired();
            builder.HasIndex(u => u.TenantId);
            
            builder.HasOne(e => e.Order)
                   .WithMany(e => e.Documents)
                   .HasForeignKey(e => e.OrderId);
            
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