using Scrutor;

namespace BuildingBlocks.Extensions.Types;

public static class ScrutorExtensions
{
    /// <summary>
    /// This extension method for <see cref="IServiceTypeSelector"/> allows selecting services that implement a specific interface.
    /// </summary>
    /// <param name="selector">The service type selector.</param>
    /// <param name="type">The interface type to match.</param>
    /// <returns>An <see cref="ILifetimeSelector"/> that can be further configured to specify the lifetime of the selected services.</returns>
    public static ILifetimeSelector AsImplementationOfInterface(this IServiceTypeSelector selector, Type type)
    {
        return selector.As(t => t.GetInterfaces().Where(i =>
            type.IsGenericType && i.IsGenericType && i.GetGenericTypeDefinition() == type || i == type));
    }
}