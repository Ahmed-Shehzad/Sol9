using System.Reflection;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using BuildingBlocks.Infrastructure.BackgroundServices.OutboxProcessor;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Repositories;
using BuildingBlocks.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Contexts;
using Orders.Infrastructure.Contexts.Contracts;

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
        services.AddScoped<IUnitOfWorkContext, UnitOfWork<OrdersDbContext>>();
        services.AddScoped<IUnitOfWork, UnitOfWork<OrdersDbContext>>();

        return services;
    }

    /// <summary>
    /// Adds outbox transaction services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddOutboxTransactions(this IServiceCollection services)
    {
        services.AddScoped<IOutboxRepository, OutboxRepository<OrdersDbContext>>();
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddHostedService<OutboxProcessorBackgroundService>();

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
        services.AddDbContext<OrdersDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(OrdersDbContextFactory).Assembly.ToString());
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), Array.Empty<string>());
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextExtensions.GetDefaultSchema<OrdersDbContext>());
            });
        });
        services.AddDbContext<OrdersReadOnlyDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
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