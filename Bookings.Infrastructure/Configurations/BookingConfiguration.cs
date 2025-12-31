using Bookings.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        _ = builder.ToTable("Bookings");

        _ = builder.HasKey(b => b.Id);

        _ = builder.Property(b => b.OrderId)
            .IsRequired();

        _ = builder.Property(b => b.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        _ = builder.Property(b => b.Status)
            .HasMaxLength(100)
            .IsRequired();

        _ = builder.Property(b => b.CreatedAtUtc);
        _ = builder.Property(b => b.UpdatedAtUtc);
        _ = builder.Property(b => b.DeletedAtUtc);
        _ = builder.Property(b => b.IsDeleted);
    }
}
