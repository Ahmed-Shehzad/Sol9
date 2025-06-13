using Microsoft.EntityFrameworkCore;
using Transponder.Storage.Npgsql.Converters;
using Transponder.Storage.Outbox;

namespace Transponder.Storage.Npgsql.Contexts;

public class TransponderDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    public TransponderDbContext(DbContextOptions<TransponderDbContext> options) : base(options)
    {
        
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Ulid>().HaveConversion<UlidToStringConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("transponder");
    }
}