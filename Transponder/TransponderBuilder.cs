using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Transponder.Abstractions;

namespace Transponder;

public class TransponderBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<Assembly> _assemblies;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TransponderBuilder"/> class,
    /// binding it to the provided dependency‑injection container.  
    /// Use this builder to configure and register Transponder handlers, behaviors,
    /// and related services before the container is built.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> that Transponder components will be
    /// added to and configured within.
    /// </param>
    public TransponderBuilder(IServiceCollection services)
    {
        _services = services;
        _assemblies = [];
    }
    
    /// <summary>
    /// Adds an assembly for Transponder handler registrations.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly"/> to register.</param>
    public void RegisterFromAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
    }
    
    public IReadOnlyList<Assembly> GetAssemblies()
    {
        return _assemblies.AsReadOnly();
    }
    
    public void Build()
    {
        _services.AddScoped<IBusPublisher, BusPublisher>();
        
        _services.Scan(scan => scan
            .FromAssemblies(_assemblies)
            
            // Register IIntegrationEventHandler<>
            .AddClasses(classes => classes.AssignableToAny(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );
    }
}