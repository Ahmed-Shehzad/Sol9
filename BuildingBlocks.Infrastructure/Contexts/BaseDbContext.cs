﻿using System.Text.Json;
using BuildingBlocks.Domain.Aggregates.Entities;
using BuildingBlocks.Infrastructure.EntityConfigurations;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Interceptors;
using BuildingBlocks.Utilities.Converters;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Contexts;

public abstract class BaseDbContext<TContext>(DbContextOptions<TContext> options, Ulid? tenantId, Ulid? userId)
    : DbContext(options)
    where TContext : DbContext
{
    public DbSet<Outbox>? OutboxMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new EntityAuditInterceptor());

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

        modelBuilder.ApplyAllConfigurations<TContext>();
        modelBuilder.ApplyDateTimeUtcConversion();
        modelBuilder.ApplyConfiguration(new OutboxConfiguration());

        modelBuilder.UseCustomNamingConvention();
        modelBuilder.UseNpgsqlDictionaryConvention();

        modelBuilder.UseNpgsqlSpatialData();
        modelBuilder.UseNpgsqlCryptoData();
        
        modelBuilder.UseNpgsqlNamingConvention();

        modelBuilder.ApplyTenantQueryFilter(tenantId);
        modelBuilder.ApplyUserQueryFilter(userId);

        modelBuilder.ApplySoftDeleteQueryFilter();
        modelBuilder.ApplyEnumerationConfiguration();
    }
}