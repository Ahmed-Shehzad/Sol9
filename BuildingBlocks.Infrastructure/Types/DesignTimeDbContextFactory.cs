using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Infrastructure.Types;

public abstract class DesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext> where TContext: DbContext
{

    public TContext CreateDbContext(string[] args)
    {
        return Create(Directory.GetCurrentDirectory(),
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), 
            "DefaultConnection");
    }
    
    protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

    protected abstract void ConfigureOptions(string connectionString, DbContextOptionsBuilder<TContext> optionsBuilder);
        
    private TContext Create(string basePath, string? env, string connectionName)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables();
        
        if (!string.IsNullOrWhiteSpace(env)) 
            builder.AddJsonFile($"appsettings.{env}.json", true);

        var config = builder.Build();

        var connectionString = config.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Could not found a connection string named '{connectionName}'");

        return Create(connectionString);
    }
    
    private TContext Create(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException($"{nameof(connectionString)} is null or empty", nameof(connectionString));

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        ConfigureOptions(connectionString, optionsBuilder);
        var options = optionsBuilder.Options;
        return CreateNewInstance(options);
    }
}