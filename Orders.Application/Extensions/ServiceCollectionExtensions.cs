using System.Reflection;
using BuildingBlocks.Extensions.Types;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Orders.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds FluentValidation services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method adds all the validators found in the assembly of the calling class to the service collection.
    /// </remarks>
    public static IServiceCollection UseFluentValidation(this IServiceCollection services)
    {
        var executingAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        if (executingAssembly.IsNullOrWhiteSpace())
            services.AddValidatorsFromAssembly(Assembly.Load(executingAssembly!));

        return services;
    }
}