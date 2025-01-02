using System.Globalization;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.Infrastructure.BackgroundServices.OutboxProcessor;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Contexts;
using Orders.Infrastructure.Contexts.Contracts;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Hangfire.Redis.StackExchange;
using Npgsql;
using Orders.Infrastructure.Repositories;
using Orders.Infrastructure.Repositories.Contracts;
using Orders.Infrastructure.Services;
using Orders.Infrastructure.Services.Contracts;

namespace Orders.Application.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to add various services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds repository services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderOutboxRepository, OrderOutboxRepository>();
        services.AddScoped<IUnitOfWorkContext, UnitOfWork<OrdersDbContext>>();
        services.AddScoped<IUnitOfWork, UnitOfWork<OrdersDbContext>>();

        return services;
    }

    /// <summary>
    /// Adds outbox transaction services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration"></param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddOutboxTransactions(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddScoped<IOrderOutboxRepository, OrderOutboxRepository>();
        services.AddScoped<IOrderOutboxService, OrderOutboxService>();
        
        // Add Hangfire services
        services.AddHangfire(options =>
        {
            options.UsePostgreSqlStorage(bootstrapperOptions =>
                {
                    bootstrapperOptions.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
                })
                .UseDashboardMetrics()
                .UseRedisStorage(configuration["REDIS_CONNECTION_STRING"]);
        });

        // Add Hangfire server
        services.AddHangfireServer();
        services.AddScoped<OutboxProcessorJob>();

        return services;
    }

    /// <summary>
    /// Adds database context services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The ConfigurationManager to use for retrieving connection strings.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddDbContexts(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddScoped<NpgsqlConnection>(_ =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var sqlConnection = new NpgsqlConnection(connectionString);
            return sqlConnection;
        });

        services.AddEntityFrameworkNpgsql().AddDbContext<OrdersDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                sqlOptions.MigrationsAssembly(typeof(OrdersDbContextFactory).Assembly.ToString());
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), Array.Empty<string>());
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextExtensions.GetDefaultSchema<OrdersDbContext>());
                sqlOptions.UseNetTopologySuite();
            });
        });

        services.AddEntityFrameworkNpgsql().AddDbContext<OrdersReadOnlyDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                sqlOptions.MigrationsAssembly(typeof(OrdersDbContextFactory).Assembly.ToString());
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), Array.Empty<string>());
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextExtensions.GetDefaultSchema<OrdersDbContext>());
                sqlOptions.UseNetTopologySuite();
            });
        });

        services.AddScoped<IOrdersReadOnlyDbContext, OrdersReadOnlyDbContext>();
        return services;
    }

    /// <summary>
    /// Adds application services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.Scan(scan =>
        {
            scan.FromAssembliesOf(typeof(OrderApplicationService))
                .AddClasses(classes => classes.AssignableTo(typeof(INotificationHandler<>)))
                .AsImplementationOfInterface(typeof(INotificationHandler<>)).WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
                .AsImplementationOfInterface(typeof(IRequestHandler<,>)).WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                .AsImplementationOfInterface(typeof(IValidator<>)).WithScopedLifetime();
        });
        return services;
    }
}