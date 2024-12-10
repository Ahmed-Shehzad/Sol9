using System.Collections;
using System.Reflection;

namespace BuildingBlocks.Extensions.Types;

public static class TypeExtensions
{
    /// <summary>
    ///     Checks if a type is nullable.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNullable(this Type? type)
    {
        if (type is null) return true;
        
        var typeInfo = type.GetTypeInfo();
        return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof (Nullable<>);
    }
        
    /// <summary>
    ///     Checks if a type is enumerable excluding strings
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsEnumerable(this Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }
}