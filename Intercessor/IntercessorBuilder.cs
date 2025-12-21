using System.Reflection;
using Intercessor.Abstractions;
using Intercessor.Behaviours;
using Microsoft.Extensions.DependencyInjection;
using Verifier;
using Verifier.Abstractions;

namespace Intercessor;

/// <summary>
/// Provides a fluent builder for configuring and registering Intercessor-related services,
/// such as request handlers, notification handlers, and pipeline behaviors, 
/// into the application's dependency injection container.
/// </summary>
public class IntercessorBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<Assembly> _assemblies;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntercessorBuilder"/> class,
    /// binding it to the provided dependency‑injection container.  
    /// Use this builder to configure and register Intercessor handlers, behaviors,
    /// and related services before the container is built.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> that Intercessor components will be
    /// added to and configured within.
    /// </param>
    public IntercessorBuilder(IServiceCollection services)
    {
        _services = services;
        _assemblies = [];
    }

    /// <summary>
    /// Adds an assembly for Intercessor handler registrations.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly"/> to register.</param>
    public void RegisterFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        _assemblies.Add(assembly);
    }

    /// <summary>
    /// Finalizes the Intercessor registration by adding core services and scanning the configured assemblies
    /// for Intercessor handler implementations. This includes:
    /// <list type="bullet">
    ///   <item><description><see cref="INotification"/>, <see cref="ISender"/>, <see cref="IPublisher"/> core services</description></item>
    ///   <item><description><see cref="IQueryHandler{TRequest, TResponse}"/></description></item>
    ///   <item><description><see cref="ICommandHandler{TRequest, TResponse}"/></description></item>
    ///   <item><description><see cref="ICommandHandler{TRequest}"/></description></item>
    ///   <item><description><see cref="IPipelineBehavior{TRequest, TResponse}"/></description></item>
    ///   <item><description><see cref="IPipelineBehavior{TRequest}"/></description></item>
    ///   <item><description><see cref="ValidationBehavior{TRequest, TResponse}"/></description></item>
    ///   <item><description><see cref="ValidationBehavior{TRequest}"/></description></item>
    ///   <item><description><see cref="IValidator{T}"/></description></item> and
    ///   <item><description><see cref="INotificationHandler{TNotification}"/> implementations via Scrutor assembly scanning</description></item>
    /// </list>
    /// This method is called internally after configuration via <c>AddMediatR(cfg => ...)</c>.
    /// </summary>
    internal void Build()
    {
        _services.AddScoped<ISender, Sender>();
        _services.AddScoped<IPublisher, Publisher>();

        var distinctAssemblies = _assemblies.Distinct().ToList();
        
        _services.Scan(scan => scan
            .FromAssemblies(distinctAssemblies)

            // Register IQueryHandler<,>
            .AddClasses(classes => classes.AssignableToAny(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()

            // Register ICommandHandler<>
            .AddClasses(classes => classes.AssignableToAny(typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()

            // Register ICommandHandler<,>
            .AddClasses(classes => classes.AssignableToAny(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()

            // Register INotificationHandler<>
            .AddClasses(classes => classes.AssignableToAny(typeof(INotificationHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()

            // Register IPipelineBehavior<>
            .AddClasses(classes => classes.AssignableToAny(typeof(IPipelineBehavior<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()

            // Register IPipelineBehavior<,>
            .AddClasses(classes => classes.AssignableToAny(typeof(IPipelineBehavior<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );

        _services.AddVerifier(x =>
        {
            foreach (Assembly assembly in distinctAssemblies) x.RegisterFromAssembly(assembly);
        });

        // Register Validation Pipeline Behavior
        _services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register Validation Pipeline Behavior
        _services.AddTransient(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));
    }
}