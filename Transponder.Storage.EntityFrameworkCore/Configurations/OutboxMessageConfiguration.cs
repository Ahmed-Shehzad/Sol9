using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transponder.Storage.Outbox;

namespace Transponder.Storage.EntityFrameworkCore.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id);
        
        builder.Property(x => x.Type);
        builder.Property(x => x.Data);
        
        builder.Property(x => x.CreatedDateUtcAt);
        builder.Property(x => x.CreatedTimeUtcAt);
        
        builder.Property(x => x.UpdatedDateUtcAt);
        builder.Property(x => x.UpdatedTimeUtcAt);
        
        builder.Property(x => x.DeletedDateUtcAt);
        builder.Property(x => x.DeletedTimeUtcAt);
        builder.Property(x => x.IsDeleted);
        
        builder.Property(x => x.PublishedDateUtcAt);
        builder.Property(x => x.PublishedTimeUtcAt);
        
        builder.Property(x => x.Error);
        
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}