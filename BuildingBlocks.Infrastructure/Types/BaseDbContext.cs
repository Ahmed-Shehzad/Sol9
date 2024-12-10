using System.Text.Json;
using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Interceptors;
using BuildingBlocks.Utilities.Converters;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Types;

public abstract class BaseDbContext<TContext>(DbContextOptions<TContext> options, Ulid? tenantId, Ulid? userId) : DbContext(options)
    where TContext : DbContext
{
    public DbSet<Outbox>? OutboxMessages { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new EntityAuditInterceptor());
        optionsBuilder.AddInterceptors(new AutoAssignDependentsInterceptor(tenantId, userId));

        // Enable sensitive data logging for debugging purposes.
        optionsBuilder.EnableSensitiveDataLogging();

        // Enable detailed error messages for better troubleshooting.
        optionsBuilder.EnableDetailedErrors();

        // Enable thread safety checks to prevent concurrent access issues.
        optionsBuilder.EnableThreadSafetyChecks();

        // Enable service provider caching to improve performance.
        optionsBuilder.EnableServiceProviderCaching();
        
        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToStringConverter>();
        
        configurationBuilder.Properties<JsonElement?>().HaveConversion<JsonElementConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureDefaultSchema(this);
        modelBuilder.UseSingularTableNamingConvention();
        
        modelBuilder.ApplyDateTimeUtcConversion();
        modelBuilder.ApplyEnumToStringConversion();
        
        modelBuilder.UseCustomNamingConvention();
        modelBuilder.UseNpgsqlDictionaryConvention();
        modelBuilder.UseNpgsqlSpatialData();
        modelBuilder.UseNpgsqlNamingConvention();
        
        modelBuilder.ApplySoftDeleteQueryFilter();
    }
}