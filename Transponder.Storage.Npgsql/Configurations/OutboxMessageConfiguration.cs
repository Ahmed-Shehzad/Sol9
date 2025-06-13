using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transponder.Storage.Outbox;

namespace Transponder.Storage.Npgsql.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        
        builder.Property(x => x.Type).HasColumnName("type");
        builder.Property(x => x.Data).HasColumnName("data");
        
        builder.Property(x => x.CreatedDateUtcAt).HasColumnName("created_date_utc_at");
        builder.Property(x => x.CreatedTimeUtcAt).HasColumnName("created_time_utc_at");
        
        builder.Property(x => x.UpdatedDateUtcAt).HasColumnName("updated_date_utc_at");
        builder.Property(x => x.UpdatedTimeUtcAt).HasColumnName("updated_time_utc_at");
        
        builder.Property(x => x.DeletedDateUtcAt).HasColumnName("deleted_date_utc_at");
        builder.Property(x => x.DeletedTimeUtcAt).HasColumnName("deleted_time_utc_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        
        builder.Property(x => x.PublishedDateUtcAt).HasColumnName("published_date_utc_at");
        builder.Property(x => x.PublishedTimeUtcAt).HasColumnName("published_time_utc_at");
        
        builder.Property(x => x.Error).HasColumnName("error");
    }
}