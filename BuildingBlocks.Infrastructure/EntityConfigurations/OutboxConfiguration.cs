using BuildingBlocks.Domain.Aggregates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.EntityConfigurations;

public class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.ToTable(nameof(Outbox));
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type);
        builder.Property(e => e.Payload);
        builder.Property(e => e.Processed);
    }
}